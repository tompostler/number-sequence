FROM ubuntu
RUN export DEBIAN_FRONTEND=noninteractive && apt update && apt-get install --yes texlive texlive-latex-extra