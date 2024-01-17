# SAPeador
Librearía de manejo de cliente SAP GUI por scripting. 

Ejecución en secuencia de distintos comandos, lecturas y escrituras 
sobre la interfaz gráfica para automatizar exportaciones de 
resultados de transacciones y otras tareas.

Al utilizar directamente la interfaz cliente de SAP, esta librería está
pensada para simplificar el uso de la herramienta de scripting para usuarios
que quieran automatizar tareas repetitivas, o que necesiten integrar el cliente
de SAP en particular a otras herramientas como PowerBI o Excel. También es
útil en caso de necesitar desarrollar una aplicación o software que precise la
integración con SAP y no se disponga de otras librerías a fin como SAP Connector.

## Uso básico de la librería
Luego de importar la librería como referencia en su proyecto,
puede instanciar la clase SapOperator con su string de conexión a SAP:
```csharp
using SAPeador;

// main class declaration and other verbosities

var oper = new SapOperator("/M/example.host.com/S/1111/G/PUBLIC", false);
```
Dicho string variará según su organización. En caso de ser una conexión directa
por Single Sign-On deberá indicar true en el parámetro "useSso".

Luego defina una secuencia a ejecutar. En el siguiente ejemplo, dejamos
todos los parámetros por default a excepción de las credenciales de acceso,
ya que el ingreso a SAP no será por sso:
```csharp
var seq = new Sequence("JDoe","Secret123");
```

Incluya en la sequencia la lista de acciones a ejecutar. En el siguiente 
ejemplo ejecutaremos la transacción "IW28", pondremos el valor "JDoe" en el
campo de "Creado por", enviaremos la tecla "F8" para ejecutarla y por último
recuperaremos la tabla resultante:
```csharp
var export = new ExportGridToExcelExecutable("wnd[0]/usr/cntlGRID1/shellcont/shell");
seq.Actions.Add(new StartTransactionExecutable("IW28"));
seq.Actions.Add(new SetTextExecutable("wnd[0]/usr/ctxtERNAM-LOW", "JDoe"));
seq.Actions.Add(new SendVKeyExecutable(SAPVirtualKey.F8));
seq.Actions.Add(export);
```

Guardamos referencias a las acciones que nos interesa leer a posterior. En este caso,
guardamos la acción que exporta una tabla a Excel para obtener dicho archivo. Para finalizar,
ejecutamos la secuencia con el objeto SapOperator y leemos lo resultante:
```csharp
oper.PlaySequence(seq);
byte[] fileBytes = Convert.FromBase64String(export.ExportedFile.FileData);
var filePath = System.IO.Path.Combine(
    Environment.CurrentDirectory,
    $"EXPORT.{export.ExportedFile.FileExtension}");
File.WriteAllBytes(filePath, fileBytes);
```

Hay varias formas de obtener los IDs de los distintos componentes del cliente SAP GUI, como por
ejemplo grabar un script utilizando la herramienta interna del cliente. Por conveniencia, la librería
incluye un ejecutable que al darle un ID, recupera una lista de hijos del componente indicado por el ID
junto con sus datos. Si no conoce ningún ID por dónde comenzar a explorar, puede hacerlo utilizando el
ID de la sesión en la que está trabajando (registrada en el campo SessionId del objeto Sequence trás la
primera ejecución), lo que le dará los objetos más superiores en el árbol de componentes de su sesión actual.


## Pre-requisitos
La librería requiere los siguientes programas instalados:
- .NET Framework 4.8 (Runtime)
- SAP GUI Client (Probado con SAP Logon 750)
- AutoIt (Requerido para algunas acciones, en especial para la exportación de tablas)

Se recomienda utilizar la misma arquitectura entre todos los programas (32bits o 64bits).
