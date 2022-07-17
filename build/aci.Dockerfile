# https://releases.ubuntu.com/22.04/ is Jammy Jellyfish
# https://mcr.microsoft.com/en-us/product/dotnet/aspnet/tags
FROM mcr.microsoft.com/dotnet/aspnet:6.0-jammy

# Install texlive components for pdflatex
RUN export DEBIAN_FRONTEND=noninteractive \
    && apt update \
    && apt-get install --yes texlive texlive-latex-extra

# Copy in the code
RUN mkdir /code
COPY content/ /code/

# Set up to run the web app
# https://docs.microsoft.com/en-us/azure/app-service/quickstart-custom-container?tabs=dotnet&pivots=container-linux-vscode
ENV PORT 8080
EXPOSE 8080
ENV ASPNETCORE_URLS "http://*:${PORT}"
ENTRYPOINT ["dotnet", "/code/number-sequence.dll"]
