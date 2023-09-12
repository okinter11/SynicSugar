namespace SynicSugar.RTC {
    public class AudioInputDeviceInfo{
        public string DeviceId { get; private set; }
        public string DeviceName { get; private set; }
        public bool DefaultDevice { get; private set; }
        internal AudioInputDeviceInfo(Epic.OnlineServices.RTCAudio.AudioInputDeviceInfo? info){
            DeviceId = info?.DeviceId;
            DeviceName = info?.DeviceName;
            DefaultDevice = info?.DefaultDevice ?? false;
        }
    }
    public class AudioOutputDeviceInfo{
        public string DeviceId { get; private set; }
        public string DeviceName { get; private set; }
        public bool DefaultDevice { get; private set; }
        internal AudioOutputDeviceInfo(Epic.OnlineServices.RTCAudio.AudioOutputDeviceInfo? info){
            DeviceId = info?.DeviceId;
            DeviceName = info?.DeviceName;
            DefaultDevice = info?.DefaultDevice ?? false;
        }
    }
}