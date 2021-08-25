﻿using System;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;

namespace chia.dotnet
{
    /// <summary>
    /// Wraps a Coloured Coin wallet
    /// </summary>
    public sealed class ColouredCoinWallet : Wallet
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="walletId">The wallet_id to wrap</param>
        /// <param name="walletProxy">Wallet RPC proxy to use for communication</param>
        public ColouredCoinWallet(uint walletId, WalletProxy walletProxy)
            : base(walletId, walletProxy)
        {
        }

        /// <summary>
        /// Get the name of a wallet's coloured coin
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>The coin name</returns>
        public async Task<string> GetName(CancellationToken cancellationToken = default)
        {
            dynamic data = new ExpandoObject();
            data.wallet_id = WalletId;

            var response = await WalletProxy.SendMessage("cc_get_name", data, cancellationToken);

            return response.name.ToString();
        }

        /// <summary>
        /// Set the name of a wallet's coloured coin
        /// </summary>
        /// <param name="name">The new name</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        public async Task SetName(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException($"{nameof(name)} cannot be null or empty");
            }

            dynamic data = new ExpandoObject();
            data.wallet_id = WalletId;
            data.name = name;

            _ = await WalletProxy.SendMessage("cc_set_name", data, cancellationToken);
        }

        /// <summary>
        /// Get the colour of a wallet's coloured coin
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>The colour as a string</returns>
        public async Task<string> GetColour(CancellationToken cancellationToken = default)
        {
            dynamic data = new ExpandoObject();
            data.wallet_id = WalletId;

            var response = await WalletProxy.SendMessage("cc_get_colour", data, cancellationToken);

            return response.colour;
        }

        /// <summary>
        /// Spend a coloured coin
        /// </summary>
        /// <param name="innerAddress">The inner address for the spend</param>
        /// <param name="amount">The amount to put in the wallet (in units of mojos)</param> 
        /// <param name="fee">The fee to create the wallet (in units of mojos)</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>A <see cref="TransactionRecord"/></returns>
        public async Task<TransactionRecord> Spend(string innerAddress, ulong amount, ulong fee, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(innerAddress))
            {
                throw new ArgumentNullException($"{nameof(innerAddress)} cannot be null or empty");
            }

            dynamic data = new ExpandoObject();
            data.wallet_id = WalletId;
            data.inner_address = innerAddress;
            data.amount = amount;
            data.fee = fee;

            return await WalletProxy.SendMessage<TransactionRecord>("cc_spend", data, "transaction", cancellationToken);
        }
    }
}
