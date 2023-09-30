FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
ARG TARGETARCH
COPY . ./
RUN dotnet restore "ToothPick/ToothPick.csproj" -a $TARGETARCH
RUN dotnet publish "ToothPick/ToothPick.csproj" -a $TARGETARCH -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG TARGETARCH
WORKDIR /app
EXPOSE 80

RUN apt update
RUN apt install -y xvfb
RUN apt install -y tar 
RUN apt install -y unzip 
RUN apt install -y xz-utils 
RUN apt install -y curl 
RUN apt install -y wget
RUN apt install -y gnupg2
RUN apt install -y python3 
RUN apt install -y ffmpeg
RUN apt clean -y

RUN wget https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp &&\
 mkdir /root/yt-dlp -p &&\
 mv yt-dlp /root/yt-dlp &&\
 chmod -R 755 /root/yt-dlp &&\
 /root/yt-dlp/yt-dlp --update-to nightly

COPY --from=publish /app/publish .
ENV PATH "$PATH:/root/yt-dlp"

ENTRYPOINT ["dotnet", "ToothPick.dll"]