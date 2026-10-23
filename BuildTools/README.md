# Сборка BeeSwarm под Windows

## 1. Плеер (Unity batchmode, хост 86zag)

```bash
cd /home/lira/BeeSwarm
setsid nohup env DISPLAY=:0 /home/lira/Unity/Hub/Editor/6000.0.60f1/Editor/Unity \
  -batchmode -quit -projectPath /home/lira/BeeSwarm \
  -executeMethod BeeSwarmBuilder.BuildWindows \
  -buildTarget StandaloneWindows64 \
  -logFile /tmp/build-win.log < /dev/null > /tmp/build-win.out 2>&1 &
```

Готово = в логе `[BUILD] target=StandaloneWindows64 result=Succeeded`, артефакты в `Builds/Windows/`.

Важно: сцена `Assets/Scenes/MainScene.unity` в репозитории пустая (0 GameObject).
`BeeSwarmBuilder` сначала собирает её методом `BeeSwarmSceneBuilder.BuildMainScene`
(reflection, метод private), сохраняет и только потом вызывает `BuildPipeline`.

## 2. Модуль Windows Build Support

Unity Hub CLI отказывается ставить модули в редактор, поставленный не через Hub
(`No modules found for this editor`). Ставим вручную:

```bash
curl -sL -o /tmp/win-mono.pkg \
  "https://download.unity3d.com/download_unity/61dfb374e36f/MacEditorTargetInstaller/UnitySetup-Windows-Mono-Support-for-Editor-6000.0.60f1.pkg"
cd /tmp && 7z x -y win-mono.pkg -opkgx && 7z x -y "pkgx/Payload~" -opkgx/out
cp -a /tmp/pkgx/out/. /home/lira/Unity/Hub/Editor/6000.0.60f1/Editor/Data/PlaybackEngines/WindowsStandaloneSupport/
```

## 3. Инсталлер (NSIS, собирается на Linux)

```bash
cd /home/lira/BeeSwarm
makensis -V2 BuildTools/bee.nsi
# → Builds/Installer/BeeSwarm-Setup-<версия>.exe
```

NSIS ставится из apt (`apt-get install -y nsis`). Пути в `.nsi` абсолютные:
makensis разрешает относительные пути от каталога скрипта, а не от cwd.

## Проверка артефакта

```bash
file Builds/Installer/BeeSwarm-Setup-*.exe   # PE32, Nullsoft Installer self-extracting archive
7z l Builds/Installer/BeeSwarm-Setup-*.exe   # список файлов и суммарный размер
```
