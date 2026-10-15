# Quickstarts: escenarios de quicktest para RimWorld

Sáltate los menús y entra directo en una colonia ya montada como la necesitas. Quickstarts sustituye el botón de quicktest de RimWorld por tus propios escenarios, escritos en C# y lanzados desde el menú de desarrollador o desde un parámetro de línea de comandos. Sin selector de escenario, sin planeta, sin barajar colonos, sin elegir sitio de aterrizaje.

```csharp
public class TinyColony : AbstractQuickstart {
  public override TaggedString description => "One small map, three colonists, paused.";

  public override int mapSize => 50;

  public override void PrepareColonists(List<Pawn> pawns) {
    foreach (Pawn pawn in pawns) {
      pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
    }
  }
}
```

## Para quién es esto

Para autores de mods. Quickstarts no añade nada a una partida normal, así que no hay motivo para suscribirse salvo que estés creando o probando un mod.

## ¿En qué se diferencia del quicktest del juego base?

El botón de quicktest normal te deja en un mapa aleatorio con colonos aleatorios y sin forma de cambiar ninguna de las dos cosas. Cada arranque es una colonia distinta, así que no puedes probar lo mismo dos veces.

Quickstarts pone en ese botón un selector con todos los quickstarts que definen los mods cargados. Elige uno y ya estás en el mapa, en pausa, con la colonia configurada.

Fija la semilla del mundo y dos ejecuciones te darán el mismo planeta, la misma casilla de aterrizaje y los mismos colonos. Un fallo se puede repetir en vez de desaparecer.

Cada subclase no abstracta aparece sola. Añadir un quickstart es escribir una clase y nada más.

## ¿Qué puedes preparar antes de que cargue el mapa?

Los hooks se ejecutan donde importa: antes de la generación, cuando ya existe el mundo, cuando el escenario termina su propia preparación y una vez que aparecen los colonos. Ajusta el número de pawns, termina investigaciones, reparte equipo o abre el panel en el que estés trabajando.

## ¿Puede correr como prueba de humo en CI?

Sí, y ese es el motivo para escribir uno. Ejecuta el mismo escenario con el parámetro de verificación y el juego comprueba sus aserciones contra la simulación en vivo, luego sale con un código de éxito o fallo. Escribe un informe JSON y también JUnit XML, así que CI puede anotar una aserción fallida en el pull request. No hace falta pantalla.

Los errores en rojo del log del juego hacen fallar la ejecución por defecto, y eso es lo que detecta una interacción rota entre mods. Un tiempo límite real detiene una ejecución atascada y dice en qué fase murió.

## Requisitos

RimWorld 1.5, 1.6 o 1.7, el modo desarrollador activado y una librería de parcheo. Sirven tanto Harmony como Concord. Quickstarts prefiere Concord si tienes las dos.

## Cómo empezar

Referencia el paquete RimWorks.Quickstarts.Ref, hereda de AbstractQuickstart y tu quickstart aparece en el selector. Para distribuir uno desde un mod sin imponer esta dependencia a tus jugadores, ponlo en una carpeta controlada por loadFolders.xml.

Código fuente y documentación: https://github.com/RimWorks/Rimworld-Quickstarts

## Más herramientas de modding de RimWorks

- [Pickle](https://steamcommunity.com/sharedfiles/filedetails/?id=3791648678): ejecuta pruebas Gherkin contra una sesión de RimWorld en marcha.
- [RimLogging](https://steamcommunity.com/sharedfiles/filedetails/?id=3733484696): visor de logs estructurado y envío de informes de error en un clic.
