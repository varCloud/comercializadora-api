---
name: convenciones-endpoints
description: Convenciones de endpoints de la API - verbos HTTP, devolver entidad, camelCase
type: decision
---

Convenciones transversales de la API (detalladas en
`.claude/arquitectura/convenciones-api.md`), establecidas por el usuario como reglas duras:

1. **Verbos HTTP correctos al migrar.** El legado usaba mal GET/POST/PUT/PATCH/DELETE; al
   portar cada operación se le asigna el verbo correcto (GET listar/consultar, POST crear,
   PUT reemplazar, PATCH parcial, DELETE eliminar; POST para comandos como login).
2. **Los controladores devuelven la entidad o `Notificacion<T>`, no `IActionResult`** (salvo
   cuando se necesite controlar el HTTP, p. ej. 201 Created). El resultado de negocio viaja
   en `Notificacion<T>` (Estatus/Mensaje), no en el código HTTP.
3. **JSON camelCase global** (`Program.cs` → `AddJsonOptions` con `CamelCase`). Modelos en
   PascalCase en C#; se serializan a camelCase. No usar `[JsonPropertyName]` salvo necesidad.

**Cómo aplicar:** seguir esto en cada módulo nuevo que se migre. Efecto colateral: respuestas
de error de negocio salen con HTTP 200 + estatus negativo (el front discrimina por el
contenido de `Notificacion`, no por el status). Ver [[arquitectura-datos]].
