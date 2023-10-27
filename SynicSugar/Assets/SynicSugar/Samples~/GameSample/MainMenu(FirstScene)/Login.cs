using Cysharp.Threading.Tasks;
using System.Threading;
using SynicSugar.Login;
using UnityEngine;
namespace  SynicSugar.Samples {
    public class Login : MonoBehaviour {
        [SerializeField] GameObject modeSelectCanvas;
        [SerializeField] bool needResultDetail;
        void Start(){
            
            bool hasLogin = EOSConnect.HasLoggedinEOS();
        #if SYNICSUGAR_FPSTEST
            if(!hasLogin){
                //Set game FPS
                Application.targetFrameRate = 60;
            }
        #endif
            
            this.gameObject.SetActive(!hasLogin);
            modeSelectCanvas.SetActive(hasLogin);
        }
        /// <summary>
        /// For button event
        /// </summary>
        public void LoginWithDeviceID(){
            LoginWithDeviceIDRequest().Forget();
        }
        public async UniTask LoginWithDeviceIDRequest(){
            this.gameObject.SetActive(false);
            EOSDebug.Instance.Log("Trt to connect EOS with deviceID.");
            //(bool, Result)
            var result = await EOSConnect.LoginWithDeviceID();
    
            if(result.isSuccess){
                modeSelectCanvas.SetActive(true);
                EOSDebug.Instance.Log("SUCCESS EOS AUTHENTHICATION!.");
                return;
            }

            //False
            this.gameObject.SetActive(true);
            EOSDebug.Instance.Log($"Fault EOS authentication. {result.detail}");
        }
        /// <summary>
        /// For button event
        /// </summary>
        public void LoginWithDevelopperTool(){
            LoginWithDevToolRequest().Forget();
        }
        public async UniTask LoginWithDevToolRequest(){
            this.gameObject.SetActive(false);
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            EOSDebug.Instance.Log("Trt to connect EOS with DevTool");
            bool isSuccess = await DevLogin.Instance.LoginWithDevelopperLogin(cancellationToken);

            if(isSuccess){
                modeSelectCanvas.SetActive(true);
                EOSDebug.Instance.Log("SUCCESS EOS AUTHENTHICATION!.");
                return;
            }
            this.gameObject.SetActive(true);
            EOSDebug.Instance.Log("Fault EOS authentication.");
        }
    }
}
