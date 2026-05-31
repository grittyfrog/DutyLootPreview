{
  description = "DutyLootPreview";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
    dalamud-distrib-repo = {
      url = "github:goatcorp/dalamud-distrib";
      flake = false;
    };
  };

  outputs = { self, nixpkgs, flake-utils, dalamud-distrib-repo }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs {
          inherit system;
        };

        mkShell = pkgs.mkShell.override {
          stdenv = pkgs.clangStdenv;
        };

        dotnet = pkgs.dotnet-sdk_10;

        dalamud-channel = ""; # See https://github.com/goatcorp/dalamud-distrib

        # Once nix flakes support zip files with top-level folders we can remove this and just point
        # the flake straight at the zip file.
        dalamud-distrib = pkgs.runCommand "dalamud-distrib" { buildInputs = [ dalamud-channel pkgs.unzip ]; } ''
          unzip ${dalamud-distrib-repo}${dalamud-channel}/latest.zip -d $out
        '';

      in {
        devShell = mkShell {
          buildInputs = [
            pkgs.roslyn-ls
            dotnet

            # For github workflow
            pkgs.jq
            pkgs.zip
          ];

          DOTNET_ROOT=dotnet;
          DALAMUD_HOME="${dalamud-distrib}";
          # Roslyn LSP — editors use Microsoft.CodeAnalysis.LanguageServer
          # with --stdio. lsp-mode's lsp-roslyn.el is pinned to an obsolete
          # named-pipe handshake, so the Doom config overrides its
          # `lsp-roslyn--connect` to use stdio (see
          # ~/devbox-public/modules/programs/emacs/doom.d/modules/csharp.el).
          # ROSLYN_LS_DLL is still needed so lsp-roslyn's `:test?` activation
          # check (which calls `f-exists?` on the dll) passes.
          ROSLYN_LS_EXE="${pkgs.roslyn-ls}/bin/Microsoft.CodeAnalysis.LanguageServer";
          ROSLYN_LS_DLL="${pkgs.roslyn-ls}/lib/roslyn-ls/Microsoft.CodeAnalysis.LanguageServer.dll";
        };
      }
    );
}
