# HEKATE-QA Azure DevOps Extension

Esta carpeta contiene el Hub inicial de Azure DevOps.

Ubicacion del Hub:

```text
Azure Boards -> HEKATE-QA
```

Por ahora carga:

```text
http://localhost:4200
```

Antes de abrirlo en Azure DevOps, la app local debe estar corriendo:

```bash
dotnet run --project ../Presentation/Api --launch-profile http
cd ../web
npm start
```
