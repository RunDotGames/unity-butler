ECHO OFF

echo "Downloading Latest Butler"
curl -L -o butler.zip https://broth.itch.ovh/butler/windows-amd64/LATEST/archive/default

echo "Extracting"
if not exist  "%UNITY_APP_PATH%/RDG/UnityButler/bin" mkdir "%UNITY_APP_PATH%/RDG/UnityButler/bin"
tar -x -C %UNITY_APP_PATH%/RDG/UnityButler/bin -f butler.zip
del butler.zip

echo "Ignoring From Repo"
echo * 1> %UNITY_APP_PATH%/RDG/UnityButler/.gitignore

PAUSE

