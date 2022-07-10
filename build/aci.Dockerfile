FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine

# Install texlive components for pdflatex
# (and wget for net60 install)
RUN export DEBIAN_FRONTEND=noninteractive \
    && apt update \
    && apt-get install --yes texlive texlive-latex-extra wget
