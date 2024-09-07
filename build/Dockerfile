# https://releases.ubuntu.com/24.04/ is Noble Numbat
# https://mcr.microsoft.com/en-us/product/dotnet/aspnet/tags
FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble

# Install texlive components for pdflatex
RUN export DEBIAN_FRONTEND=noninteractive \
    && apt update \
    && apt-get install --yes texlive texlive-latex-extra \
    && apt-get clean

# Copy in the code
RUN mkdir /code
COPY content/ /code/

# Set up to run the web app
# https://docs.microsoft.com/en-us/azure/app-service/quickstart-custom-container?tabs=dotnet&pivots=container-linux-vscode
ENV PORT 8080
EXPOSE 8080
ENV ASPNETCORE_URLS "http://*:${PORT}"
ENTRYPOINT ["dotnet", "/code/number-sequence.dll"]
