# chia-dotnet
A [.net 5](https://dotnet.microsoft.com/download/dotnet/5.0) client library for [chia](https://github.com/Chia-Network/chia-blockchain) RPC interfaces that runs on linux and windows.

![build](https://github.com/dkackman/chia-dotnet/actions/workflows/dotnet.yml/badge.svg)

## Build 

````bash
dotnet build ./src/chia-dotnet.sln
````

## Tests

Various unit and integration tests in the test project. Tests attributes with `[TestCategory("Integration")]` will use the local install of chia and the mainnet configuration to resolve RPC endpoints. 
