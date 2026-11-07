using System;
using WheelOfFortune.Data;

namespace WheelOfFortune.Core
{
    public interface IWheelSpinController
    {
        bool IsSpinning { get; }
        WheelConfigSO CurrentConfig { get; }
        IWheelModel CurrentModel { get; }
        event Action<int> OnSlicePassed;
        event Action<int> OnSpinCompleted;

        void Initialize(WheelConfigSO config, int zoneMultiplier);
        void Initialize(IWheelModel wheelModel, int zoneMultiplier);
        void SpinToSlice(int targetIndex, Action<int> onCompleted = null);
        void StopImmediate();
    }
}
