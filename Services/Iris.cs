using Aware.NexaIrisApi;
using System;
using System.Collections.Generic;
using static Aware.NexaIrisApi.NexaIris;
using System.Configuration;
using System.Threading;

namespace apidemo.Services
{
    public class Iris
    {
        private NexaIris nexaIris = null;
        private CacheConfig cacheConfig = null;
        private bool initialized;
        private List<Algorithm> algorithms = null;
        private float _score;
        private string _error;
        AutoResetEvent stopWaitHandle = new AutoResetEvent(false);
        private string ChachePath { get { return ConfigurationManager.AppSettings["irisCacheDir"]; } }
        public Iris()
        {
            nexaIris = new NexaIris();
            algorithms = new List<Algorithm>();
            cacheConfig = nexaIris.CreateCacheConfig("cache");
            cacheConfig.SetDir(ChachePath);
            _error = string.Empty;
            initialized = false;
        }
        public string PhotoOne { get; set; }
        public string PhotoTwo { get; set; }
        public float Verify(string id)
        {
            Initialize();
            var probe = CreateEncounter();
            var gallery = id;
            var workflow = CreateCompareWorkflow();
            nexaIris.Verify("verify_", probe, gallery, workflow, "cache");
            stopWaitHandle.WaitOne(); // wait for callback    
            if (!string.IsNullOrWhiteSpace(_error))
            {
                throw new Exception(_error);
            }
            return _score;

        }
        public float Compare()
        {
            Initialize();
            var probe = CreateEncounter(PhotoOne, IrisType.INFRARED_LEFT);
            var gallery = CreateEncounter(PhotoTwo, IrisType.INFRARED_LEFT);
            var workflow = CreateCompareWorkflow();
            nexaIris.Compare("compare_", probe, gallery, workflow);
            stopWaitHandle.WaitOne();
            if (!string.IsNullOrWhiteSpace(_error))
            {
                throw new Exception(_error);
            }
            probe = CreateEncounter(PhotoOne, IrisType.INFRARED_RIGHT);
            gallery = CreateEncounter(PhotoTwo, IrisType.INFRARED_RIGHT);
            nexaIris.Compare("compare_", probe, gallery, workflow);
            stopWaitHandle.WaitOne();
            return _score;
        }
        private void OnCompareResult(
         string jobId,
         ErrorInfo.errorCode errorCode,
         CompareResult result)
        {
            if (errorCode != ErrorInfo.errorCode.AW_NEXA_IRIS_E_NO_ERRORS)
            {
                _error = NexaIris.GetErrorDetails(errorCode);
            }
            else
            {
                _score = _score > result.GetScore() ? _score : result.GetScore();
            }
            stopWaitHandle.Set();
        }
        /// <summary>
        /// Initialize the nexaIris instance if it has not already been
        /// initialized.
        /// </summary>
        private void Initialize()
        {
            if (!initialized)
            {
                algorithms.Add(NexaIris.Algorithm.I500);

                foreach (NexaIris.Algorithm algorithm in
                          Enum.GetValues(typeof(NexaIris.Algorithm)))
                {
                    nexaIris.EnableAlgorithm(algorithm);
                    if (IntPtr.Size == 4)
                    {
                        cacheConfig.AddAlgorithm(algorithm, NexaIris.CacheType.DISK);
                    }
                    else
                    {
                        cacheConfig.AddAlgorithm(algorithm, NexaIris.CacheType.RAM);
                    }
                }
                foreach (NexaIris.IrisType iris in
                          Enum.GetValues(typeof(NexaIris.IrisType)))
                {
                    cacheConfig.EnableIris(iris);
                }
                nexaIris.AddCache(cacheConfig);
                nexaIris.SetCompareResultCallback(OnCompareResult);
                nexaIris.Initialize();
            }
            initialized = true;
        }
        private IrisType[] GetIrises()
        {
            return (NexaIris.IrisType[])Enum.GetValues(typeof(NexaIris.IrisType));
        }
        private Encounter CreateEncounter()
        {
            Encounter encounter = nexaIris.CreateEncounter();
            encounter.SetIris(IrisType.INFRARED_LEFT, Convert.FromBase64String(PhotoOne));
            encounter.SetIris(IrisType.INFRARED_RIGHT, Convert.FromBase64String(PhotoTwo));
            encounter.SetId("test");
            return encounter;
        }
        private Encounter CreateEncounter(string base64, IrisType type)
        {
            Encounter encounter = nexaIris.CreateEncounter();
            encounter.SetIris(type, Convert.FromBase64String(base64));
            encounter.SetId("test");
            return encounter;
        }
        private Workflow CreateCompareWorkflow()
        {
            Workflow workflow = nexaIris.CreateWorkflow();
            workflow.SetComparator(Algorithm.I500, GetIrises());
            return workflow;
        }
    }
}