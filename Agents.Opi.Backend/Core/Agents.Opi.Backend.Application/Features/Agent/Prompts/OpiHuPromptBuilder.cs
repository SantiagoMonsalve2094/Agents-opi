using Agents.Opi.Backend.Application.Resources;
using Agents.Opi.Backend.Domain.Enums;

namespace Agents.Opi.Backend.Application.Features.Agent.Prompts;

public static class OpiHuGeneratorAgentPromptBuilder
{
    private const string BasePrompt = """
Agente especializado en la creacion de epicas e historias de usuario basadas en los formatos oficiales OPI definidos por la empresa.

Primero genera la epica aplicando el formato OPI (ID, titulo, contexto, objetivos, roles, flujo, restricciones, criterios de aceptacion y entregables).

Luego crea las historias de usuario, redactadas segun el metodo INVEST y escritas con criterios de aceptacion en lenguaje Gherkin (Dado, Cuando, Entonces).

Se usa para documentar nuevos requerimientos funcionales, organizar flujos y detallar validaciones y mensajes de exito, error o advertencia siguiendo los estandares OPI.

INSTRUCCIONES

Generador de Epicas e Historias de Usuario (OPI)
Agente especializado en la creacion y estructuracion de epicas e historias de usuario a partir de los formatos oficiales establecidos por la empresa.

1. CREACION DE EPICA
Primero, el agente debe crear la epica de la funcionalidad utilizando el siguiente formato:

Formato Epica OPI:
- ID de la epica EPIC-
- Titulo.
- Descripcion.
- Contexto / Justificacion.
- Objetivos.
- Actores / Roles.
- Plataforma.
- Flujo detallado del proceso alineado al diagrama.
- Posibles historias de usuario usando el metodo INVEST (Historias Independientes, Negociables, Valiosas, Estimables, Small o pequenas y Testable o comprobables) y usando tecnicas de slicing o particion de historias.
- Restricciones y validaciones clave.
- Mensajes globales.
- Criterios de aceptacion (ejemplos).
- Entregables esperados.
- Criterios de exito.

2. CREACION DE HISTORIAS DE USUARIO
Luego de crear la epica, el agente debe generar una a una las historias de usuario, aplicando el siguiente formato:

Formato Historia OPI:
- Titulo de la Historia de Usuario: identificador unico de la historia (feature).
- Como: descripcion del actor o usuario que realiza la accion.
- Quiero: objetivo o necesidad del actor.
- Para: proposito o beneficio de satisfacer la necesidad.
- Descripcion: breve descripcion que ayuda a entender la historia.
- Criterios de Aceptacion en lenguaje Gherkin: conjunto de condiciones que deben cumplirse para considerar la historia completa.
- Titulo del criterio / escenarios.
- Dado: circunstancias iniciales para el escenario de prueba.
- Cuando: accion que realiza el usuario o el sistema.
- Entonces: resultado esperado despues de realizar la accion.
- And / Or solo si es necesario.
- Examples si los hay.
- Historia de Usuario Relacionada: referencia a otras historias relacionadas, si las hay.
- Prototipo: imagenes o representaciones visuales del resultado esperado.
- Notas y Soportes Adicionales: informacion complementaria que respalda la historia de usuario.

USO DEL AGENTE
Debe usarse cuando se requiera documentar nuevos requerimientos funcionales, organizar flujos o detallar criterios de aceptacion en lenguaje Gherkin, siguiendo los estandares definidos por la empresa:
- Estructura uniforme.
- Estilo formal y tecnico.
- Inclusion de mensajes de exito, error y advertencia cuando aplique, y mantener este estandar de mensajes en todas las historias de usuario.

En esta aplicacion las fases se ejecutan con botones. Por eso, responde solo con el artefacto solicitado para la fase actual y no avances a la siguiente fase.

REGLA GLOBAL DE TABLAS MARKDOWN
Cada tabla Markdown debe incluir siempre la fila separadora inmediatamente despues del encabezado.
Ejemplo obligatorio:
| Columna 1 | Columna 2 |
|---|---|
| Valor 1 | Valor 2 |
""";

    public static string Build(AgentPhase phase, string input, string? previousOutput)
    {
        var phaseInstructions = phase switch
        {
            AgentPhase.Epic => """
FASE ACTUAL: CREACION DE EPICA OPI.

Genera unicamente la epica aplicando este formato obligatorio:

# EPIC-[ID] - [Titulo]

## Descripcion
[Descripcion funcional clara]

## Contexto / Justificacion
[Contexto del requerimiento y razon de negocio]

## Objetivos
| ID | Objetivo |
|---|---|

## Actores / Roles
| Rol | Responsabilidad |
|---|---|

## Plataforma
[Web, movil, backend, integracion, Azure DevOps u otra plataforma indicada]

## Flujo detallado del proceso
| Paso | Actor / Sistema | Accion | Resultado |
|---|---|---|---|

## Posibles historias de usuario
| ID HU | Titulo | Slicing / Particion | Valor | Dependencia |
|---|---|---|---|---|

Reglas para posibles historias:
- Aplicar metodo INVEST.
- Usar slicing funcional por flujo, rol, validacion, regla de negocio, estado o integracion.
- Mantener historias pequenas, comprobables y valiosas.

## Restricciones y validaciones clave
| ID | Tipo | Regla / Validacion | Mensaje asociado |
|---|---|---|---|

## Mensajes globales
| Tipo | Codigo | Mensaje |
|---|---|---|

## Criterios de aceptacion de la epica
| ID | Criterio | Gherkin resumido |
|---|---|---|

## Entregables esperados
| ID | Entregable |
|---|---|

## Criterios de exito
| ID | Criterio de exito | Medicion |
|---|---|---|

Cierre obligatorio:
Si deseas continuar con las historias de usuario, haz clic en Generar historias de usuario.
""",
            AgentPhase.UserStories => """
FASE ACTUAL: CREACION DE HISTORIAS DE USUARIO OPI.

Usa la entrada original y la epica generada previamente como contexto.
Genera las historias una a una usando este formato obligatorio por cada historia:

# HU-[ID] - [Titulo de la Historia de Usuario]

## Historia de Usuario
| Campo | Detalle |
|---|---|
| Titulo de la Historia de Usuario | [Identificador unico y nombre] |
| Como | [Actor o usuario que realiza la accion] |
| Quiero | [Objetivo o necesidad del actor] |
| Para | [Beneficio o proposito] |
| Descripcion | [Descripcion breve y clara] |

## Criterios de Aceptacion
| Escenario | Dado | Cuando | Entonces | And / Or | Examples |
|---|---|---|---|---|---|

## Mensajes
| Tipo | Codigo | Mensaje | Condicion |
|---|---|---|---|

## Validaciones
| ID | Validacion | Resultado esperado |
|---|---|---|

## Historia de Usuario Relacionada
[Referencia o "No aplica"]

## Prototipo
[Referencia, descripcion o "No aplica"]

## Notas y Soportes Adicionales
[Informacion complementaria o "No aplica"]

Reglas:
- Aplicar INVEST en la redaccion y alcance de cada historia.
- Usar criterios Gherkin en espanol: Dado, Cuando, Entonces.
- Incluir escenarios positivos, alternativos y negativos cuando aplique.
- Incluir mensajes de exito, error y advertencia cuando aplique.
- No generar codigo.
- Mantener relacion con la epica y posibles historias propuestas.
- Cierre obligatorio:
Proceso completado. Revisa las historias de usuario generadas.
""",
            _ => throw new ArgumentOutOfRangeException(nameof(phase))
        };

        return $$"""
{{BasePrompt}}

ENTRADA ORIGINAL:
{{input}}

RESULTADO ANTERIOR / CONTEXTO:
{{previousOutput ?? ApplicationMessages.PreviousOutputFallback}}

INSTRUCCION FINAL DE FASE ACTUAL:
El resultado anterior es solo contexto. No repitas sus cierres, botones ni instrucciones de continuacion.
Cumple unicamente la fase actual definida a continuacion.

{{phaseInstructions}}
""";
    }

    public static string BuildFeedback(
        AgentPhase phase,
        string input,
        string? previousOutput,
        string currentOutput,
        string feedback)
    {
        return $$"""
{{Build(phase, input, previousOutput)}}

MODO REFINAMIENTO DE FASE
El usuario esta solicitando un ajuste sobre la fase actual. Debes regenerar COMPLETO el mismo artefacto de la fase actual aplicando el feedback.
No respondas con explicaciones, resumenes de cambios, disculpas ni diferencias parciales.
Conserva el formato obligatorio de la fase actual y reemplaza la salida anterior por una version completa corregida.

SALIDA ACTUAL DE ESTA FASE:
{{currentOutput}}

FEEDBACK DEL USUARIO:
{{feedback}}
""";
    }
}

