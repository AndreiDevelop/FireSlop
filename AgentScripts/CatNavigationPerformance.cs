using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FireSlop.Interaction;
using FireSlop.Cats;
using Newtonsoft.Json;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace FireSlop.Verification
{
    public static class CatNavigationPerformance
    {
        public static async Task<string> Sample(string label, int frames, bool moveFurniture)
        {
            if (!Application.isPlaying)
            {
                throw new InvalidOperationException("Sample in Play Mode.");
            }

            var handles = new List<ProfilerRecorderHandle>();
            ProfilerRecorderHandle.GetAvailable(handles);
            var descriptions = handles.Select(ProfilerRecorderHandle.GetDescription).ToArray();
            var navDescription = descriptions.First(d => d.Name == "NavMeshManager");
            var mainDescription = descriptions.First(d => d.Name == "Main Thread");
            var areas = Object.FindObjectsByType<CatPatrolArea>();
            var bodies = Object.FindObjectsByType<Movable>().Where(m => areas.Any(a => a.Contains(m.transform.position)))
                .Take(3).Select(m => m.GetComponent<Rigidbody>()).ToArray();
            var origins = bodies.Select(b => b.position).ToArray();
            var kinematic = bodies.Select(b => b.isKinematic).ToArray();
            var navSamples = new double[frames];
            var mainSamples = new double[frames];
            var patrolSamples = new double[frames];
            var patrolDescription = descriptions.First(d => d.Name == "Cat.Patrol");
            using (var nav = ProfilerRecorder.StartNew(navDescription.Category, navDescription.Name, 1))
            using (var main = ProfilerRecorder.StartNew(mainDescription.Category, mainDescription.Name, 1))
            using (var patrol = ProfilerRecorder.StartNew(patrolDescription.Category, patrolDescription.Name, 1))
            {
                try
                {
                    if (moveFurniture)
                    {
                        foreach (var body in bodies)
                        {
                            body.isKinematic = true;
                        }
                    }
                    for (var index = 0; index < frames; index++)
                    {
                        if (moveFurniture)
                        {
                            for (var bodyIndex = 0; bodyIndex < bodies.Length; bodyIndex++)
                            {
                                bodies[bodyIndex].position = origins[bodyIndex] +
                                    Vector3.right * (Mathf.Sin(index * 0.12f) * 0.4f);
                            }
                        }
                        var frame = Time.frameCount;
                        while (Time.frameCount == frame)
                        {
                            await Task.Yield();
                        }
                        navSamples[index] = nav.LastValue / 1000000d;
                        mainSamples[index] = main.LastValue / 1000000d;
                        patrolSamples[index] = patrol.LastValue / 1000000d;
                    }
                    Array.Sort(navSamples);
                    Array.Sort(mainSamples);
                    Array.Sort(patrolSamples);
                    var result = new
                    {
                        label,
                        frames,
                        moveFurniture,
                        obstacleCount = Object.FindObjectsByType<NavMeshObstacle>().Length,
                        navRecorderValid = nav.Valid,
                        mainRecorderValid = main.Valid,
                        navMeanMs = navSamples.Average(),
                        navP95Ms = navSamples[(int)(frames * 0.95f)],
                        navMaxMs = navSamples[frames - 1],
                        patrolMeanMs = patrolSamples.Average(),
                        patrolP95Ms = patrolSamples[(int)(frames * 0.95f)],
                        mainMeanMs = mainSamples.Average(),
                        mainP95Ms = mainSamples[(int)(frames * 0.95f)]
                    };
                    var json = JsonConvert.SerializeObject(result);
                    File.WriteAllText("/tmp/fireslop-cat-cooldown/" + label + ".json", json);
                    return json;
                }
                finally
                {
                    if (moveFurniture)
                    {
                        for (var index = 0; index < bodies.Length; index++)
                        {
                            bodies[index].position = origins[index];
                            bodies[index].isKinematic = kinematic[index];
                        }
                    }
                }
            }
        }
    }
}
