{
  lib,
  appimageTools,
  fetchurl,
  makeDesktopItem,
  version,
  sha256,
}:
let
  src = fetchurl {
    url = "https://github.com/overcookedspaghetti/Depthstrap/releases/download/v${version}/Depthstrap-linux-x64.AppImage";
    inherit sha256;
  };

  appimageContents = appimageTools.extractType2 {
    inherit version src;
    pname = "depthstrap";
  };

  desktopItem = makeDesktopItem {
    name = "depthstrap";
    desktopName = "Depthstrap";
    comment = "A cross-platform Roblox bootstrapper, focused on performance and customization.";
    exec = "depthstrap %u";
    icon = "depthstrap";
    categories = [ "Game" ];
    mimeTypes = [
      "x-scheme-handler/roblox"
      "x-scheme-handler/roblox-player"
      "x-scheme-handler/roblox-studio"
      "x-scheme-handler/roblox-studio-auth"
    ];
  };
in
appimageTools.wrapType2 {
  pname = "depthstrap";
  inherit version src;

  extraPkgs =
    pkgs: with pkgs; [
      icu
      dbus
    ];

  # list values must end with ";". makeDesktopItem omits it.
  extraInstallCommands = ''
    install -Dm644 ${desktopItem}/share/applications/depthstrap.desktop \
        $out/share/applications/depthstrap.desktop

    sed -i -E '/^(Categories|MimeType)=/ s/([^;])$/\1;/' \
        $out/share/applications/depthstrap.desktop

    install -Dm644 ${appimageContents}/usr/share/icons/hicolor/scalable/apps/Depthstrap.png \
        $out/share/icons/hicolor/512x512/apps/depthstrap.png
  '';

  meta = {
    description = "A cross-platform Roblox bootstrapper, focused on performance and customization.";
    homepage = "https://depthstrap.xyz/";
    license = lib.licenses.mpl20;
    platforms = [ "x86_64-linux" ];
  };
}
