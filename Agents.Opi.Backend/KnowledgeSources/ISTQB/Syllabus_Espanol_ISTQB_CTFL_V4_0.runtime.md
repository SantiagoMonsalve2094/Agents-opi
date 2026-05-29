# Syllabus ISTQB CTFL v4.0 - conocimiento compacto para HEKATE-QA

Esta version compacta se deriva del syllabus ISTQB CTFL v4.0 incluido en KnowledgeSources/ISTQB y se usa como contexto operativo para evitar exceder limites TPM del modelo.

## Principios de testing

- El testing muestra la presencia de defectos, no su ausencia.
- Las pruebas exhaustivas son impracticables; se debe priorizar por riesgo y valor.
- El testing temprano reduce costo y esfuerzo de correccion.
- Los defectos tienden a agruparse en zonas concretas del producto.
- Las pruebas deben revisarse y variar para evitar el efecto pesticida.
- El testing depende del contexto del producto, dominio, criticidad y tecnologia.
- Ausencia de defectos no implica utilidad si el producto no satisface necesidades del usuario.

## Proceso fundamental de testing

- Planificacion: definir alcance, objetivos, recursos, riesgos, enfoque, criterios de entrada/salida y estrategia.
- Monitoreo y control: comparar avance real contra plan, ajustar actividades y comunicar estado.
- Analisis: revisar base de prueba, identificar condiciones de prueba y riesgos.
- Diseno: derivar casos de prueba, datos, ambiente y trazabilidad desde condiciones.
- Implementacion: preparar procedimientos, suites, datos y ambiente ejecutable.
- Ejecucion: correr pruebas, registrar resultados, comparar con esperados y reportar defectos.
- Finalizacion: consolidar evidencias, lecciones aprendidas, metricas y cierre.

## Trazabilidad

- Mantener relacion entre requisito, condicion de prueba, caso de prueba, procedimiento, resultado y defecto.
- La trazabilidad permite medir cobertura, impacto de cambios y completitud de pruebas.
- En HEKATE-QA debe mantenerse trazabilidad 1:1 entre ISTQB, Gherkin tabular, codigo Gherkin y Selenium.

## Niveles y tipos de prueba

- Componentes: unidades o modulos individuales.
- Integracion: interacciones entre componentes, servicios, APIs o sistemas.
- Sistema: comportamiento end-to-end del sistema completo.
- Aceptacion: validacion frente a necesidades de negocio o usuario.
- Funcionales: que hace el sistema.
- No funcionales: rendimiento, seguridad, usabilidad, compatibilidad, confiabilidad, mantenibilidad, portabilidad.
- Caja negra: basada en especificacion o comportamiento observable.
- Caja blanca: basada en estructura interna.
- Relacionadas con cambios: confirmacion y regresion.

## Tecnicas de caja negra requeridas

### Particion de equivalencia

- Divide datos o comportamientos en grupos equivalentes.
- Se selecciona al menos un representante valido y uno invalido por particion relevante.
- Es util para campos obligatorios, formatos validos/invalidos, estados de cuenta y credenciales.

### Analisis de valores limite

- Se enfoca en bordes de rangos, longitudes, cantidades e intentos.
- Cubrir valores justo por debajo, en el limite y justo por encima cuando la regla exista.
- Si la regla no esta definida, no inventarla; marcarla como no definida o como supuesto explicito.
- Para bloqueo tras 3 intentos: probar 2 intentos, 3 intentos y acceso posterior bloqueado.

### Tablas de decision

- Adecuadas cuando el resultado depende de combinaciones de condiciones.
- Usar para combinaciones como correo valido/invalido, contrasena correcta/incorrecta, cuenta activa/bloqueada y campos vacios.
- Cada regla debe mapear condiciones a acciones esperadas.

### Transicion de estados

- Adecuada cuando el sistema cambia de estado por eventos.
- Usar para cuenta activa, intentos fallidos, cuenta bloqueada, recuperacion solicitada y acceso posterior.
- Verificar estados previos, evento, estado final y acciones visibles.

## Buenas practicas para casos ISTQB

- Cada caso debe ser independiente, claro, repetible y verificable.
- Incluir precondiciones, datos, pasos numerados, resultado esperado, prioridad y tecnica.
- No duplicar escenarios con distinto nombre si validan lo mismo.
- Priorizar por riesgo: autenticacion valida, bloqueo, credenciales incorrectas y recuperacion son alta prioridad.
- No inventar funcionalidades no mencionadas por el requisito. CAPTCHA, 2FA, bloqueo por IP, usuario inactivo o reglas de contrasena solo deben aparecer si la entrada las menciona.
- Si falta detalle, formular resultado esperado como regla no definida o comportamiento pendiente de confirmacion.

## BDD y Gherkin

- Given describe contexto/precondiciones.
- When describe acciones del usuario o evento.
- Then describe resultado verificable.
- And complementa Given, When o Then sin mezclar responsabilidades.
- Un escenario Gherkin debe corresponder a un caso ISTQB por ID.
- Mantener lenguaje de negocio, no detalles tecnicos innecesarios.

## Automatizacion Selenium

- Automatizar escenarios estables, repetibles y de alto valor.
- Usar Page Object y patron Screenplay para separar intenciones del actor, tareas, preguntas y localizadores.
- Usar WebDriverWait y selectores robustos.
- Evitar sleeps fijos salvo justificacion.
- Mantener trazabilidad del ID del caso en comentarios o nombres de prueba.
- No depender de datos ambiguos; parametrizar usuarios, contrasenas y URLs.

## Criterios de calidad del artefacto

- Tablas Markdown deben iniciar y terminar cada fila con pipe.
- No usar tecnicas fuera de: Particion de equivalencia, Valores limite, Tabla de decisiones, Transicion de estados.
- Mantener flujos: Flujo Principal, Flujo Alternativo, Flujo Negativo.
- Resultado esperado debe ser observable y verificable.
- Cuando la historia esta incompleta, continuar con reservas sin inventar reglas de negocio.
