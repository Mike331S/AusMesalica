using Common;

namespace dCom.ViewModel
{
    internal abstract class AnalogBase : BasePointItem, IAnalogPoint
    {
        private double eguValue;

        //Polja za cuvanje parametara
        private double scaleFactor;
        private double deviation;
        private double highLimit;
        private double lowLimit;

        public AnalogBase(IConfigItem c, IProcessingManager processingManager, IStateUpdater stateUpdater, IConfiguration configuration, int i)
            : base(c, processingManager, stateUpdater, configuration, i)
        {
            //Ucitavanje parametara iz konfiguracije
            this.scaleFactor = c.ScaleFactor;
            this.deviation = c.Deviation;
            this.highLimit = c.HighLimit;
            this.lowLimit = c.LowLimit;
        }

        public double EguValue
        {
            get => eguValue;
            set
            {
                eguValue = value;
                OnPropertyChanged("DisplayValue");
            }
        }

        //Implementacija novih propertija
        public double ScaleFactor => scaleFactor;
        public double Deviation => deviation;
        public double HighLimit => highLimit;
        public double LowLimit => lowLimit;

        public override string DisplayValue => EguValue.ToString();

        protected override bool WriteCommand_CanExecute(object obj)
        {
            return false;
        }
    }
}