---
name: bd-local-desarrollo
description: BD local de desarrollo - instancia, base y autenticación para comercializadora-api
type: reference
---

Base de datos local de desarrollo (réplica de la legada) para `comercializadora-api`:

- **Instancia:** `localhost\SQLEXPRESS01`  *(ojo: SQLEXPRESS01, no SQLEXPRESS)*.
- **Base de datos:** `DB_A57E86_comercializadora`.
- **Autenticación:** SQL Server auth, usuario `sa`. La **contraseña va en User Secrets**
  (`ConnectionStrings:DefaultConnection`), **nunca** versionada en `appsettings.json`.
- Otras bases en la instancia: `db_a86705_moreliapre`, `efact`, `test-miracion`.

**Cómo aplicar:** la cadena `DefaultConnection` se configura con
`dotnet user-secrets`. Para SqlClient moderno suele requerir `TrustServerCertificate=True`
(certificado self-signed local). La BD legada productiva está en `SQL5063.site4now.net`.

**SP de login:** `SP_VALIDA_CONTRASENA(@usuario varchar(200), @contrasena varchar(40),
@macAdress varchar(100)=null)`. En éxito (status=200) devuelve 4 resultsets: header
(status/mensaje), sesión (1 fila), permisos (PermisosRolPorModulo + módulo), y
`FactConfiguracionComprobante` (empresa: Rfc/Nombre/Telefono/Domicilio). En error, solo el
header. Validación de contraseña en texto plano dentro del SP (deuda técnica conocida).
