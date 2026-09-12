# TobonMouse

> Utilidad para Windows que activa el comportamiento **x-mouse**: la ventana que está bajo el
> ratón recibe el foco sin necesidad de hacer clic («focus follows mouse» en Unix y Linux).
> También permite que la ventana enfocada se traiga al frente (*raising*) y ajustar el retardo.

**Build propia** basada en [X-Mouse Controls](https://joelpurra.com/projects/X-Mouse_Controls/)
de [Joel Purra](https://joelpurra.com/), publicado bajo **GNU GPL v3**. Ver
[Licencia y créditos](#licencia-y-créditos) antes de redistribuirlo.

| | |
| --- | --- |
| Plataforma | Windows 10 / 11 |
| Runtime | .NET 8 (framework-dependent) |
| Interfaz | Español |
| Icono en la bandeja | Sí |
| Arranque con Windows | Opcional, desde la propia ventana |

## Qué hace

El ajuste x-mouse de Windows se controla con tres valores del sistema que esta aplicación lee y
escribe a través de `SystemParametersInfo`:

| Ajuste | Qué controla |
| --- | --- |
| `SPI_ACTIVEWINDOWTRACKING` | Que la ventana bajo el ratón reciba el foco sin clic |
| `SPI_ACTIVEWNDTRKZORDER` | Que la ventana enfocada se traiga al frente automáticamente |
| `SPI_ACTIVEWNDTRKTIMEOUT` | El retardo en milisegundos antes de cambiar el foco (0—2500, por defecto 500) |

**Retardo recomendado: 100—300 ms.** Con valores muy bajos algunos menús y ventanas emergentes
desaparecen antes de recibir el foco.

Los cambios se aplican y se guardan al instante (son ajustes del usuario, no hace falta
ejecutar como administrador). La ventana se mantiene siempre encima para no perderse debajo de
otras, y muestra el estado real del sistema: si algo lo cambia por fuera, la interfaz se
actualiza sola.

## Uso

- **Abrir**: doble clic en el icono de la bandeja.
- **Cerrar la ventana (X)**: no sale; la aplicación se queda en la bandeja para que el ajuste
  siga activo. La primera vez avisa con un globo.
- **Salir de verdad**: clic derecho en el icono → *Salir*.
- **Iniciar con Windows**: marca la casilla y la aplicación se registra en
  `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` con el argumento `-tray`, de modo que
  arranca directamente en la bandeja, sin abrir la ventana.

## Compilar

Requiere el [SDK de .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0).

```powershell
git clone https://github.com/danieltobon21/xmouse-controls.git
cd xmouse-controls\Code   # o la carpeta que contenga el .sln
dotnet publish "X-Mouse Controls\TobonMouse.csproj" -c Release -o ..\dist
```

La salida es `dist\TobonMouse.exe` más sus dependencias: cópiala a una carpeta de usuario y
ejecuta el `.exe`. Para actualizar, cierra la aplicación desde la bandeja y sobrescribe los
archivos.

## Qué cambia respecto al original

| Tema | Detalle |
| --- | --- |
| **.NET 8** | El proyecto original apunta a **.NET Framework 3.5**, sin soporte y sin *targeting packs* en máquinas modernas (las referencias `v3.5` no están instaladas por defecto). Migrado a `net8.0-windows` (WPF), así compila con solo el SDK de .NET. |
| **Bandeja del sistema** | Icono con menú (abrir / salir), doble clic para abrir y cierre que oculta en lugar de terminar el proceso. |
| **Arranque con Windows** | Casilla en la ventana, con el valor `TobonMouse` en la clave `Run` del usuario y arranque silencioso (`-tray`). |
| **Interfaz en español** | Textos traducidos (el original está en inglés). |
| **DPI** | *Manifest* con `PerMonitorV2`: la ventana se ve nítida con escalado al 125/150 %. |
| **Corrección** | El original ponía el retardo a 0 si se escribía algo no numérico en la casilla (el `TryParse` fallido dejaba la variable a cero). Ahora el texto inválido se ignora. |
| **Identidad** | Nombre, icono e información propios; los enlaces del original se mantienen como crédito. |

Lo que **no** cambia: el código que lee y escribe los ajustes con `SystemParametersInfo`
(`Helpers.cs`, `NativeMethods.cs`, `SPI.cs`, `SPIF.cs`, `WM.cs`) es el del proyecto original.
Cada archivo conserva su cabecera de copyright y lleva una nota indicando que fue modificado.

## Licencia y créditos

**GNU General Public License v3.0** — igual que el proyecto original, y esto es importante:
a diferencia de otros proyectos MIT, si distribuyes este binario estás obligado a

1. publicar el código fuente de tu versión bajo la misma licencia,
2. conservar los avisos de copyright originales («Copyright © Joel Purra 2007—2023»),
3. declarar qué cambiaste (esta versión lo hace en las cabeceras de archivo y en esta tabla).

Para uso personal no hay ninguna obligación.

TobonMouse es una build personal de **X-Mouse Controls** (Joel Purra, GPL-3.0): el mérito del
programa original, su diseño y su código de interacción con Windows son suyos. Este fork solo
aporta el paso a .NET 8, la bandeja, el arranque con Windows, la traducción y la identidad.
Los materiales de diseño originales siguen en `Graphics/` y `docs/`.
