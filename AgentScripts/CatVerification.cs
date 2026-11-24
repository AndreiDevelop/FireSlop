using System;
using FireSlop.Cats;
using FireSlop.Drone;
using FireSlop.Firefighter;
using FireSlop.Interaction;
using FireSlop.MissionHud;
using FireSlop.Tools;
using UnityEditor;

namespace FireSlop.Verification
{
    public static class CatVerification
    {
        public static string RunModelChecks()
        {
            var settings = AssetDatabase.LoadAssetAtPath<CatConfigSO>("Assets/SO/Cats/Cat.asset");
            Require(settings != null && settings.IsValid, "Authored cat settings must be valid.");
            var first = new CatModel();
            var second = new CatModel();
            first.SetThreatened(true);
            Require(first.State == CatState.Warning && second.State == CatState.Idle,
                "Each cat must own independent state.");
            Require(!first.TryJump(settings.WarningSeconds), "A warning must precede a jump.");
            first.Advance(settings.WarningSeconds);
            first.SetThreatened(false);
            Require(first.State == CatState.Idle, "Lost sight must cancel the warning.");
            first.SetThreatened(true);
            Require(first.StateSeconds == 0f, "A new warning must restart its timer.");
            Require(!first.ApplyWater(settings.WaterSeconds * 0.5f, settings.WaterSeconds),
                "A partial water hit must not finish calming.");
            Require(first.ApplyWater(settings.WaterSeconds, settings.WaterSeconds),
                "Sufficient water must calm the cat.");
            first.SetThreatened(true);
            Require(first.State == CatState.Settled && !first.TryJump(settings.WarningSeconds),
                "A calmed cat must not attack during its cooldown.");
            Require(!first.ApplyWater(1f, settings.WaterSeconds), "Calming must complete only once.");
            second.SetThreatened(true);
            second.Advance(settings.WarningSeconds);
            Require(second.TryJump(settings.WarningSeconds), "A full warning must allow jumping.");
            Require(!second.ApplyWater(1f, settings.WaterSeconds), "Water must not calm an airborne cat.");
            second.FinishJump();
            Require(second.State == CatState.Attached, "The jump must attach the cat.");

            var face = new CatFaceModel();
            Require(face.TryReserve("first"), "The first cat must reserve the face.");
            Require(!face.TryReserve("second"), "A second cat must not claim an occupied face.");
            face.Attach("second", settings);
            Require(!face.IsAttached, "A different cat must not attach to another reservation.");
            face.Attach("first", settings);
            var removals = 0;
            Action remove = () =>
            {
                removals++;
                second.Remove();
                face.Release("first");
            };
            face.RemovalRequested += remove;
            face.RequestRemoval(0d);
            face.ResetIfExpired(settings.RemovalGapSeconds + 0.1d);
            Require(face.Progress == 0f && face.IsAttached, "A pause must reset the series.");
            for (var index = 0; index < settings.RemovalPresses; index++)
            {
                face.RequestRemoval(1d + index * 0.1d);
            }
            face.RequestRemoval(5d);
            face.RemovalRequested -= remove;
            Require(removals == 1 && !face.IsOccupied && second.State == CatState.Returning,
                "Repeated removal must release exactly once.");
            second.FinishReturn();
            second.SetThreatened(true);
            Require(second.State == CatState.Settled, "A removed cat must not attack during its cooldown.");
            Require(second.TryRescue(), "A settled cat must be rescuable.");
            Require(!second.TryRescue() && second.State == CatState.Rescued,
                "Repeated delivery must not rescue a cat twice.");
            Require(!new CatModel().TryRescue(), "A hostile cat cannot be rescued.");

            var belt = AssetDatabase.LoadAssetAtPath<ToolBeltSO>("Assets/SO/ToolBelts/ToolBelt.asset");
            var tools = new ToolSelectionModel(belt);
            var input = new MissionInputModel();
            var flight = new DroneModel();
            flight.SetOnPlatform(true);
            using (var interaction = new InteractionModel(tools, input, flight, face))
            {
                face.TryReserve("priority");
                face.Attach("priority", settings);
                Require(interaction.ContextPrompt.ActionId == HudActionId.RemoveCat,
                    "An attached cat must take priority over drone controls.");
                flight.TogglePilot(0f, 1f);
                Require(interaction.ContextPrompt.ActionId == HudActionId.RemoveCat,
                    "Cat removal must remain available if flight begins during the jump.");
                interaction.SetActionsEnabled(false);
                Require(!interaction.ContextPrompt.IsAvailable, "Mission completion must hide the cat action.");
                interaction.SetActionsEnabled(true);
                Require(interaction.ContextPrompt.ActionId == HudActionId.RemoveCat,
                    "Reopening controls must read current attachment state.");
            }

            var catalog = AssetDatabase.LoadAssetAtPath<HudActionCatalogSO>("Assets/SO/HudActions/HUDActions.asset");
            catalog.Validate();
            Require(!string.IsNullOrWhiteSpace(catalog.Get(HudActionId.RemoveCat).Label),
                "Cat removal needs authored HUD text.");
            return "PASS: warning timing/cancellation, water threshold/idempotence, independent cats, " +
                "exclusive attachment, removal/retry, rescue/retry, no repeat attack, drone priority, " +
                "action gating and HUD catalog.";
        }

        public static string RunCooldownChecks()
        {
            var config = AssetDatabase.LoadAssetAtPath<CatConfigSO>("Assets/SO/Cats/Cat.asset");
            var cat = new CatModel();
            cat.ApplyWater(config.WaterSeconds, config.WaterSeconds);
            cat.Advance(config.AttackCooldownSeconds * 0.5f);
            Require(!cat.TryEndCooldown(config.AttackCooldownSeconds, false), "Cooldown must not end early.");
            cat.Advance(config.AttackCooldownSeconds);
            Require(!cat.TryEndCooldown(config.AttackCooldownSeconds, true), "A carried cat cannot rearm.");
            Require(cat.TryEndCooldown(config.AttackCooldownSeconds, false), "A free cat must rearm after cooldown.");
            Require(cat.WaterSeconds == 0f, "A new encounter must require fresh calming water.");
            cat.SetThreatened(true);
            Require(!cat.TryJump(config.WarningSeconds), "A repeat attack still needs the full warning.");
            cat.Advance(config.WarningSeconds);
            Require(cat.TryJump(config.WarningSeconds), "A rearmed cat must be able to attack again.");
            cat.FinishJump();
            cat.Remove();
            cat.FinishReturn();
            cat.Advance(config.AttackCooldownSeconds);
            Require(cat.TryEndCooldown(config.AttackCooldownSeconds, false), "Removal must also expire.");
            cat.ApplyWater(config.WaterSeconds, config.WaterSeconds);
            Require(cat.TryRescue(), "A cooling-down cat must remain rescuable.");
            cat.Advance(config.AttackCooldownSeconds);
            Require(!cat.TryEndCooldown(config.AttackCooldownSeconds, false), "A rescued cat must never rearm.");
            return "PASS: cooldown threshold, carried protection, fresh water, repeated warning/attack, removal and rescue.";
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
