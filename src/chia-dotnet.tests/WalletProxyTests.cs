﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Threading;
using System.Threading.Tasks;

namespace chia.dotnet.tests
{
    [TestClass]
    [TestCategory("Integration")]
    public class WalletProxyTests
    {
        private static Daemon _theDaemon;
        private static WalletProxy _theWallet;

        [ClassInitialize]
        public static async Task Initialize(TestContext context)
        {
            _theDaemon = Factory.CreateDaemon();

            await _theDaemon.Connect(CancellationToken.None);
            await _theDaemon.Register(CancellationToken.None);
            _theWallet = new WalletProxy(_theDaemon);
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            _theDaemon?.Dispose();
        }

        [TestMethod()]
        public async Task GetWallets()
        {
            var wallets = await _theWallet.GetWallets(CancellationToken.None);

            Assert.IsNotNull(wallets);
        }

        [TestMethod()]
        public async Task GetPublicKeys()
        {
            var keys = await _theWallet.GetPublicKeys(CancellationToken.None);

            Assert.IsNotNull(keys);
        }

        [TestMethod()]
        public async Task Login()
        {
            var fingerprints = await _theWallet.GetPublicKeys(CancellationToken.None);
            Assert.IsNotNull(fingerprints);
            Assert.IsTrue(fingerprints.Count > 0);

            var keys = await _theWallet.LogIn((uint)fingerprints[0], skipImport: true, CancellationToken.None);

            Assert.IsNotNull(keys);
        }

        [TestMethod()]
        public async Task GetPrivateKey()
        {
            var key = await _theWallet.GetPrivateKey(2287630151, CancellationToken.None);

            Assert.IsNotNull(key);
        }

        [TestMethod()]
        public async Task GetWalletBalance()
        {
            var balance = await _theWallet.GetWalletBalance(1, CancellationToken.None);

            Assert.IsNotNull(balance);
        }

        [TestMethod()]
        public async Task GetTransactions()
        {
            var transactions = await _theWallet.GetTransactions(1, CancellationToken.None);

            Assert.IsNotNull(transactions);
        }

        [TestMethod()]
        public async Task GetSyncStatus()
        {
            var info = await _theWallet.GetSyncStatus(CancellationToken.None);

            Assert.IsNotNull(info);
        }

        [TestMethod()]
        public async Task GetNetworkInfo()
        {
            var info = await _theWallet.GetNetworkInfo(CancellationToken.None);

            Assert.IsNotNull(info);
        }

        [TestMethod()]
        public async Task GetHeightInfo()
        {
            var height = await _theWallet.GetHeightInfo(CancellationToken.None);

            Assert.IsTrue(height > 0);
        }

        [TestMethod()]
        public async Task GetNextAddress()
        {
            var address = await _theWallet.GetNextAddress(1, true, CancellationToken.None);

            Assert.IsNotNull(address);
        }
    }
}
