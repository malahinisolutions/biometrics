using Aware.NexaFaceApi;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace apidemo.Services
{
    public class Face
    {
        private NexaFace nexaFace = null;
        private NexaFace.CacheConfig cacheConfig = null;
        private string cacheName;
        private bool initialized;
        private bool operationCompleted;
        private float _score;
        private string _error;
        AutoResetEvent stopWaitHandle = new AutoResetEvent(false);
        public string PhotoOne { get; set; }
        public string PhotoTwo { get; set; }
        private string ChachePath { get { return ConfigurationManager.AppSettings["faceCacheDir"]; } }
        private string OptimizeModelPath { get { return ConfigurationManager.AppSettings["faceOptimizeModelPath"]; } }
        public Face()
        {
            cacheName = "cache";
            initialized = false;
            operationCompleted = false;
            nexaFace = new NexaFace();
            cacheConfig = nexaFace.CreateCacheConfig(cacheName);
            nexaFace.SetOptimizationModel(OptimizeModelPath);
            cacheConfig.SetDir(ChachePath);
            _error = string.Empty;
        }
        /// <summary>
        /// Initialize the nexaFace instance if it has not already been
        /// initialized.
        /// </summary>
        private void Initialize()
        {
            if (!initialized)
            {
                foreach (NexaFace.Algorithm algorithm in
                          Enum.GetValues(typeof(NexaFace.Algorithm)))
                {
                    nexaFace.EnableAlgorithm(algorithm);
                    if (IntPtr.Size == 4)
                    {
                        cacheConfig.AddAlgorithm(algorithm, NexaFace.CacheType.DISK);
                    }
                    else
                    {
                        cacheConfig.AddAlgorithm(algorithm, NexaFace.CacheType.RAM);
                    }
                }
                foreach (NexaFace.FaceType face in
                          Enum.GetValues(typeof(NexaFace.FaceType)))
                {
                    cacheConfig.EnableFace(face);
                }
                nexaFace.AddCache(cacheConfig);
                nexaFace.SetCompareResultCallback(OnCompareResult);
                nexaFace.Initialize();
            }
            initialized = true;
        }
        public float Verify(string id)
        {
            Initialize();
            var probe = CreateEncounter();
            var workflow = CreateCompareWorkflow();
            nexaFace.Verify("verify_", probe, id, workflow, cacheName);
            stopWaitHandle.WaitOne(); // wait for callback   
            while (!operationCompleted) ;
            if (!string.IsNullOrWhiteSpace(_error))
            {
                throw new Exception(_error);
            }
            return _score;
        }
        public float Compare()
        {
            Initialize();
            var probe = CreateEncounter(PhotoOne);
            var gallery = CreateEncounter(PhotoTwo);
            var workflow = CreateCompareWorkflow();
            nexaFace.Compare(DateTime.Now.Ticks.ToString(), probe, gallery, workflow);
            stopWaitHandle.WaitOne(); // wait for callback    
            if (!string.IsNullOrWhiteSpace(_error))
            {
                throw new Exception(_error);
            }
            return _score;
        }
        private void OnCompareResult(string jobId, ErrorInfo.errorCode errorCode, NexaFace.CompareResult result)
        {
            if (errorCode != ErrorInfo.errorCode.AW_NEXA_FACE_E_NO_ERRORS)
            {
                _error = NexaFace.GetErrorDetails(errorCode);
            }
            else
            {
                _score = result.GetScore();
            }
            operationCompleted = true;
            stopWaitHandle.Set();
        }

        private NexaFace.Encounter CreateEncounter()
        {
            NexaFace.Encounter encounter = nexaFace.CreateEncounter();
            byte[] data = Convert.FromBase64String(PhotoOne);
            encounter.SetImage(NexaFace.FaceType.VISIBLE_FRONTAL, data);
            encounter.SetId("test");
            return encounter;
        }

        private NexaFace.Encounter CreateEncounter(string base64)
        {
            NexaFace.Encounter encounter = nexaFace.CreateEncounter();
            byte[] data = Convert.FromBase64String(base64);
            encounter.SetImage(NexaFace.FaceType.VISIBLE_FRONTAL, data);
            //encounter.SetId("test");
            return encounter;
        }
        /// <summary>
        /// Creates a workflow that can be used for any 1 to 1 comparison
        /// operation.
        /// </summary>
        /// 
        /// <returns>
        /// Workflow instance.
        /// </returns>
        private NexaFace.Workflow CreateCompareWorkflow()
        {
            NexaFace.Workflow workflow = nexaFace.CreateWorkflow();
            var faces = new NexaFace.FaceType[1];
            faces[0] = NexaFace.FaceType.VISIBLE_FRONTAL;
            workflow.SetComparator(NexaFace.Algorithm.F200, faces);
            return workflow;
        }
        private NexaFace.FaceType[] GetFaces()
        {
            return (NexaFace.FaceType[])Enum.GetValues(typeof(NexaFace.FaceType));
        }
    }
}