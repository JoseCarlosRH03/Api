# CryptoMonitor

Servicio backend en .NET 10 para monitoreo de criptomonedas. Integra CoinCap API v3, persiste datos en SQLite y expone REST API interna con detección de alertas de precio.

## Requisitos

- NET 10 SDK 10.x.x
- API Key de CoinCap v3

## Configuración inicial

1 Clona el repositorio y navega a la raíz:
2 Crea `appsettings.Development.json` con tus credenciales

ejemplo: `
{
  "CoinCap": {
    "ApiKey": "",
    "BaseUrl": ""
  },
  "ApiSecurity": {
    "ApiKey": ""
  }
}
`

## Ejecutar la API

dotnet run --project Api/src/CryptoMonitor.API

## Ejecutar tests

```bash
# Tests unitarios
dotnet test Api/tests/CryptoMonitor.UnitTests

# Tests de integración
dotnet test Api/tests/CryptoMonitor.IntegrationTests

# Tests E2E
dotnet test Api/tests/CryptoMonitor.E2ETests

# Todos con cobertura
dotnet test Api/CryptoMonitor.slnx --collect:"XPlat Code Coverage"
```

## Migraciones de base de datos

La base de datos se crea y migra automáticamente al arrancar la API. Para crear una nueva migración manualmente:

## Variables de entorno (CI/CD)

 `CoinCap__ApiKey` = API Key de CoinCap 
 `ApiSecurity__ApiKey` = API Key de los endpoints propios
 `ASPNETCORE_ENVIRONMENT` = `Development` y `Production` 
 `ConnectionStrings__Default` = Override de la cadena de conexión SQLite 

## En el caso de requerir el token de  CoinCap y Api key utilizado

Utilizar el siguiente json, este es compartido para facilitar la evaluacion: 
`
  {
    "CoinCap": {
      "ApiKey": "3ac0d28aafc3d3526c9fbdb6486765f1104f7e7a22a4fbff062043d0844c7df2",
      "BaseUrl": "https://rest.coincap.io/v3"
    },
    "ApiSecurity": {
      "ApiKey": "-ncFxn-0zudMsXcHlIZSGA5sdjSAwWzc_C_ByYw6WaM"
    }
  }
`

