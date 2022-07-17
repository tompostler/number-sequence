# https://releases.ubuntu.com/22.04/ is Jammy Jellyfish
# https://mcr.microsoft.com/en-us/product/dotnet/aspnet/tags
FROM mcr.microsoft.com/dotnet/aspnet:6.0-jammy

# Install texlive components for pdflatex
RUN export DEBIAN_FRONTEND=noninteractive \
    && apt update \
    && apt-get install --yes texlive texlive-latex-extra
