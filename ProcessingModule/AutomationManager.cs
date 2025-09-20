using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ProcessingModule
{
    /// <summary>
    /// Class containing logic for automated work.
    /// </summary>
    public class AutomationManager : IAutomationManager, IDisposable
    {
        //Definicija stanja procesa
        private enum ProcessState
        {
            IDLE,
            FILLING_CHOCOLATE,
            FILLING_MILK,
            FILLING_WATER,
            MIXING,
            EMPTYING,
            ERROR_EMPTYING
        }

        private Thread automationWorker;
        private AutoResetEvent automationTrigger;
        private IStorage storage;
        private IProcessingManager processingManager;
        private IConfiguration configuration;

        //Polja za pracenje stanja simulacije
        private ProcessState currentState = ProcessState.IDLE;
        private int mixingTimer = 0;

        //Reference na Modbus tacke koje koristimo u simulaciji
        private IDigitalPoint startSignal;
        private IDigitalPoint motor;
        private IDigitalPoint v1, v2, v3, v4;
        private IAnalogPoint kolicina;

        public AutomationManager(IStorage storage, IProcessingManager processingManager, AutoResetEvent automationTrigger, IConfiguration configuration)
        {
            this.storage = storage;
            this.processingManager = processingManager;
            this.configuration = configuration;
            this.automationTrigger = automationTrigger;
        }

        private void InitializeAndStartThreads()
        {
            InitializeAutomationWorkerThread();
            StartAutomationWorkerThread();
        }

        private void InitializeAutomationWorkerThread()
        {
            automationWorker = new Thread(AutomationWorker_DoWork);
            automationWorker.Name = "Automation Thread";
        }

        private void StartAutomationWorkerThread()
        {
            automationWorker.Start();
        }


        private void AutomationWorker_DoWork()
        {
            // Inicijalno preuzimanje referenci na tacke
            FetchPoints();

            while (!disposedValue)
            {
                // Ceka signal tajmera (svake sekunde)
                automationTrigger.WaitOne();

                if (kolicina == null) // Ako tacke jos nisu ucitane
                {
                    FetchPoints();
                    continue;
                }

                // Masina stanja
                switch (currentState)
                {
                    case ProcessState.IDLE:
                        IdleState();
                        break;
                    case ProcessState.FILLING_CHOCOLATE:
                        FillChocolateState();
                        break;
                    case ProcessState.FILLING_MILK:
                        FillMilkState();
                        break;
                    case ProcessState.FILLING_WATER:
                        FillWaterState();
                        break;
                    case ProcessState.MIXING:
                        MixingState();
                        break;
                    case ProcessState.EMPTYING:
                    case ProcessState.ERROR_EMPTYING:
                        EmptyingState();
                        break;
                }
            }
        }

        #region State Logic

        private void IdleState()
        {
            if (startSignal.State == DState.ON)
            {
                // Proces je pokrenut spolja
                WriteDigital(v1, DState.ON); // Otvori ventil za cokoladu
                currentState = ProcessState.FILLING_CHOCOLATE;
            }
        }

        private void FillChocolateState()
        {
            double newAmount = kolicina.EguValue + 50; // 50 kg/sec
            WriteAnalog(kolicina, newAmount);
            if (kolicina.EguValue >= 100)
            {
                WriteDigital(v1, DState.OFF);
                WriteDigital(v2, DState.ON);
                currentState = ProcessState.FILLING_MILK;
            }
        }

        private void FillMilkState()
        {
            double newAmount = kolicina.EguValue + 50; // 50 l/sec
            WriteAnalog(kolicina, newAmount);
            if (kolicina.EguValue >= 250) // 100 cokolade + 150 mleka
            {
                WriteDigital(v2, DState.OFF);
                WriteDigital(v3, DState.ON);
                currentState = ProcessState.FILLING_WATER;
            }
        }

        private void FillWaterState()
        {
            double newAmount = kolicina.EguValue + 30; // 30 l/sec
            WriteAnalog(kolicina, newAmount);
            if (kolicina.EguValue >= 370) // 250 + 120 vode
            {
                WriteDigital(v3, DState.OFF);
                WriteDigital(motor, DState.ON);
                mixingTimer = 0; // Resetuj tajmer
                currentState = ProcessState.MIXING;
            }
        }

        private void MixingState()
        {
            // Provera uslova za gresku
            if (v1.State == DState.ON || v2.State == DState.ON || v3.State == DState.ON)
            {
                WriteDigital(motor, DState.OFF);
                WriteDigital(v1, DState.OFF);
                WriteDigital(v2, DState.OFF);
                WriteDigital(v3, DState.OFF);
                currentState = ProcessState.ERROR_EMPTYING;
                return;
            }

            mixingTimer++;
            if (mixingTimer >= 10) // 10 sekundi
            {
                WriteDigital(motor, DState.OFF);
                WriteDigital(v4, DState.ON);
                currentState = ProcessState.EMPTYING;
            }
        }

        private void EmptyingState()
        {
            double newAmount = kolicina.EguValue - 100; // 100 kg/sec
            WriteAnalog(kolicina, newAmount);
            if (kolicina.EguValue <= 0)
            {
                WriteAnalog(kolicina, 0); // Postavi na tacno 0
                WriteDigital(v4, DState.OFF);
                WriteDigital(startSignal, DState.OFF); // Resetuj start signal
                currentState = ProcessState.IDLE;
            }
        }
        #endregion

        #region Helper Methods

        private void FetchPoints()
        {
            var allPoints = storage.GetPoints(new List<PointIdentifier>());
            startSignal = allPoints.FirstOrDefault(p => p.ConfigItem.Description == "Start") as IDigitalPoint;
            motor = allPoints.FirstOrDefault(p => p.ConfigItem.Description == "Motor") as IDigitalPoint;
            v1 = allPoints.FirstOrDefault(p => p.ConfigItem.Description == "Ventil_V1") as IDigitalPoint;
            v2 = allPoints.FirstOrDefault(p => p.ConfigItem.Description == "Ventil_V2") as IDigitalPoint;
            v3 = allPoints.FirstOrDefault(p => p.ConfigItem.Description == "Ventil_V3") as IDigitalPoint;
            v4 = allPoints.FirstOrDefault(p => p.ConfigItem.Description == "Ventil_V4") as IDigitalPoint;
            kolicina = allPoints.FirstOrDefault(p => p.ConfigItem.Description == "Kolicina_sastojaka") as IAnalogPoint;
        }

        private void WriteDigital(IDigitalPoint point, DState state)
        {
            processingManager.ExecuteWriteCommand(point.ConfigItem, 0, configuration.UnitAddress, (ushort)point.PointId, (int)state);
        }

        private void WriteAnalog(IAnalogPoint point, double value)
        {
            processingManager.ExecuteWriteCommand(point.ConfigItem, 0, configuration.UnitAddress, (ushort)point.PointId, (int)value);
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Postavljamo flag da bi se thread ugasio
                    disposedValue = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Start(int delayBetweenCommands)
        {
            InitializeAndStartThreads();
        }

        public void Stop()
        {
            Dispose();
        }
        #endregion
    }
}