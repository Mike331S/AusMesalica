using Common;

namespace dCom.ViewModel
{
    internal abstract class DigitalBase : BasePointItem, IDigitalPoint
    {
        private DState state;
        //Polje za cuvanje AbnormalValue
        private ushort abnormalValue;

        public DigitalBase(IConfigItem c, IProcessingManager processingManager, IStateUpdater stateUpdater, IConfiguration configuration, int i)
            : base(c, processingManager, stateUpdater, configuration, i)
        {
            //Ucitavanje AbnormalValue iz konfiguracije
            this.abnormalValue = c.AbnormalValue;
        }

        public DState State
        {
            get => state;
            set
            {
                state = value;
                OnPropertyChanged("State");
                OnPropertyChanged("DisplayValue");
            }
        }

        //Implementacija novog propertija
        public ushort AbnormalValue => abnormalValue;

        public override string DisplayValue => State.ToString();

        protected override bool WriteCommand_CanExecute(object obj)
        {
            return false;
        }
    }
}