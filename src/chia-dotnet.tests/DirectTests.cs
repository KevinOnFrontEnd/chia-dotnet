﻿using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace chia.dotnet.tests
{
    [TestClass]
    public class DirectTests
    {
        [TestMethod]
        [TestCategory("Integration")]
        public async Task ConnectDirectlyToFullNode()
        {
            try
            {
                var endpoint = new EndpointInfo()
                {
                    Uri = new Uri("https://172.26.210.216:8555"),
                    CertPath = @"\\wsl$/Ubuntu-20.04/home/don/.chia/mainnet/config/ssl/full_node/private_full_node.crt",
                    KeyPath = @"\\wsl$/Ubuntu-20.04/home/don/.chia/mainnet/config/ssl/full_node/private_full_node.key",
                };

                using var rpcClient = new HttpRpcClient(endpoint);
                var fullNode = new FullNodeProxy(rpcClient, "unit_tests");

                var state = await fullNode.GetBlockchainState();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }


        [TestMethod]
        [TestCategory("Integration")]
        public async Task ConnectDirectlyUsingConfigEndpoint()
        {
            try
            {
                var config = Config.Open();
                var endpoint = config.GetEndpoint("full_node");

                using var rpcClient = new HttpRpcClient(endpoint);
                var fullNode = new FullNodeProxy(rpcClient, "unit_tests");

                var state = await fullNode.GetBlockchainState();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}
