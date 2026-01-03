# Rig procedural (Unity) para pose 3D de 17 articulaciones (array 17×3)

Este repositorio implementa un “rig” mínimo **generado por código** en Unity a partir de un **array de 17 articulaciones** con coordenadas **(x, y, z)** por fotograma (estilo COCO/YOLO Pose).  
La intención es validar el flujo de una estimación de pose hacia un array 17 x 3, para tener un esqueleto en escena.

La visualización es un stickman:
- **Articulaciones**: esferas (17)
- **Huesos**: cilindros orientados y escalados dinámicamente entre pares de articulaciones
- 
Este código no genera huesos reales. Se comprueba que con **datos mínimos** (17×3 por frame) se puede reconstruir una pose 3D consistente y reproducible.

---

## 1) Datos de entrada: CSV por fotograma

El sistema consume un archivo CSV con filas:

```
frame,index,x,y,z
```

- `frame`: entero (0..N-1)
- `index`: entero (0..16)
- `x,y,z`: flotantes

El archivo de prueba viene en:

- `Assets/StreamingAssets/pose_test.csv`

### 1.1) Conversión desde array 17×3

Teniendo `J[17][3]` por fotograma, es realizada la converisón:

**Manera:**
```
for frame in frames:
  for i in 0..16:
    escribir(frame, i, J[i][0], J[i][1], J[i][2])
```

---

## 2) Índices (convención COCO/YOLO Pose, 17 puntos)

| Índice | Articulación |
|---:|---|
| 0 | Nariz |
| 1 | Ojo izquierdo |
| 2 | Ojo derecho |
| 3 | Oreja izquierda |
| 4 | Oreja derecha |
| 5 | Hombro izquierdo |
| 6 | Hombro derecho |
| 7 | Codo izquierdo |
| 8 | Codo derecho |
| 9 | Muñeca izquierda |
| 10 | Muñeca derecha |
| 11 | Cadera izquierda |
| 12 | Cadera derecha |
| 13 | Rodilla izquierda |
| 14 | Rodilla derecha |
| 15 | Tobillo izquierdo |
| 16 | Tobillo derecho |

---

## 3) Grafo de huesos (conexiones)

Para construir el esqueleto mínimo se usan estas conexiones (segmentos):

- Brazo izquierdo: 5–7–9  
- Brazo derecho: 6–8–10  
- Pierna izquierda: 11–13–15  
- Pierna derecha: 12–14–16  
- Hombros: 5–6  
- Caderas: 11–12  
- Tronco: 5–11 y 6–12  

Es posible recuperar de un array a una pose 3D relacionando topológicamente el código. 
En cada fotograma el rig:
1) Coloca cada esfera `J[i]` en su posición `(x,y,z)`  
2) Para cada hueso `(a,b)`, calcula el vector `dir = J[b] - J[a]`  
3) Coloca el cilindro en el punto medio y lo orienta con `FromToRotation(up, dir)`  
4) Ajusta su escala para que su longitud coincida con `|dir|`

---
Los datos de prueba incluyen un trackeo de 300 fotogramas, depurando sin un sensor real. 
En cuanto a la marcha, las piernas alternan entre avance y retroceso con una señal senoidal
Los brazos están en contrafase, siviendo esta como una herramienta para detectar errores. 
Un desplazamiento suave hacia delante en z, confirma la continuidad temporal
`pose_test.csv`

- **Alturas**: caderas ~1.0, hombros ~1.45, cabeza ~1.7 (unidades tipo “metros”)


---

## 5) Uso en Unity (paso a paso)

1) Abre este repositorio como proyecto en Unity (contiene `Assets/`, `Packages/`, `ProjectSettings/`).  
2) En la escena, crea un objeto vacío: `RigPose17`.  
3) Añade el componente: `RigPose17Procedural` (script en `Assets/Scripts/`).  
4) Pulsa Play.

El script crea automáticamente:
- 17 esferas (articulaciones)
- cilindros para huesos
- y reproduce el CSV.

### Controles
- Espacio: pausar / reproducir  
- Flecha izquierda / derecha: frame anterior / siguiente  

> Entrada: el script soporta tanto el Input Manager clásico como el Input System (si Unity está configurado en “Both” o en el sistema nuevo).

---

## 6) Ajustes 

En el componente `RigPose17Procedural`:

- `escala`: si tus datos están en milímetros, usa `0.001`
- `signoEjes`: para espejado/inversión. Ejemplos:
  - `(-1, 1, 1)` invierte X
  - `( 1, 1,-1)` invierte Z

---

## Archivos

- `Assets/Scripts/PoseCsvLoader.cs` — parseo CSV en frames 17×3  
- `Assets/Scripts/RigPose17Procedural.cs` — creación del rig y reproducción  
- `Assets/StreamingAssets/pose_test.csv` — dataset de prueba
