#!/bin/sh
set -eu
umask 077
data_dir="${APP_DATA_PATH:-/app/App_Data}"
case "$data_dir" in
  /*) ;;
  *) echo "APP_DATA_PATH must be absolute." >&2; exit 1 ;;
esac
case "$data_dir" in
  /|/app|/etc|/usr|/var|/tmp) echo "APP_DATA_PATH must be a dedicated data directory." >&2; exit 1 ;;
esac
mkdir -p "$data_dir"
# Railway mounts volumes as root. Change only the mount directory, not arbitrary children.
if [ "$(id -u)" = "0" ]; then
    chown app:app "$data_dir"
    exec gosu app dotnet CatalogStudio.dll
fi
exec dotnet CatalogStudio.dll
