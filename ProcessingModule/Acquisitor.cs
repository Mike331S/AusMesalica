using Common;
using System;
using System.Threading;

namespace ProcessingModule
{
    /// <summary>
    /// Class containing logic for periodic polling.
    /// </summary>
    public class Acquisitor : IDisposable
    {
        private AutoResetEvent acquisitionTrigger;
        private IProcessingManager processingManager;
        private Thread acquisitionWorker;
        private IStateUpdater stateUpdater;
        private IConfiguration configuration;
        // IZMENA: Dodajemo flag za graceful shutdown
        private bool disposed = false;

        public Acquisitor(AutoResetEvent acquisitionTrigger, IProcessingManager processingManager, IStateUpdater stateUpdater, IConfiguration configuration)
        {
            this.stateUpdater = stateUpdater;
            this.acquisitionTrigger = acquisitionTrigger;
            this.processingManager = processingManager;
            this.configuration = configuration;
            this.InitializeAcquisitionThread();
            this.StartAcquisitionThread();
        }

        #region Private Methods

        private void InitializeAcquisitionThread()
        {
            this.acquisitionWorker = new Thread(Acquisition_DoWork);
            this.acquisitionWorker.Name = "Acquisition thread";
        }

        private void StartAcquisitionThread()
        {
            acquisitionWorker.Start();
        }

        /// <summary>
        /// Acquisitor thread logic.
        /// </summary>
		private void Acquisition_DoWork()
        {
            // Implementacija logike periodicnog ocitavanja
            while (!disposed)
            {
                // Ceka signal od tajmera (svake sekunde)
                acquisitionTrigger.WaitOne();

                // Prolazi kroz sve grupe tacaka definisane u konfiguraciji
                foreach (var configItem in configuration.GetConfigurationItems())
                {
                    // Uvecava brojac sekundi za svaku grupu
                    configItem.SecondsPassedSinceLastPoll++;

                    // Ako je proslo dovoljno vremena, salje komandu za ocitavanje
                    if (configItem.SecondsPassedSinceLastPoll >= configItem.AcquisitionInterval)
                    {
                        processingManager.ExecuteReadCommand(
                            configItem,
                            configuration.GetTransactionId(),
                            configuration.UnitAddress,
                            configItem.StartAddress,
                            configItem.NumberOfRegisters);

                        // Resetuje brojac
                        configItem.SecondsPassedSinceLastPoll = 0;
                    }
                }
            }
        }

        #endregion Private Methods

        public void Dispose()
        {
            //Postavljamo flag da bi se thread ugasio
            disposed = true;
        }
    }
}