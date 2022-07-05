FROM ubuntu:22.04

# Install texlive components for pdflatex
# (and wget for net60 install)
RUN export DEBIAN_FRONTEND=noninteractive \
    && apt update \
    && apt-get install --yes texlive texlive-latex-extra wget

# https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu
RUN wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb
RUN apt-get update \
    && apt-get install --yes apt-transport-https \
    && apt-get update \
    && apt-get install --yes aspnetcore-runtime-6.0
