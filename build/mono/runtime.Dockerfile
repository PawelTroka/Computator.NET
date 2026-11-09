# Isolated end-user dependency proof, deliberately excludes all SDKs/compilers.
ARG UBUNTU_VERSION=24.04
FROM ubuntu:${UBUNTU_VERSION}
ARG UBUNTU_VERSION
RUN apt-get update && \
    case "$UBUNTU_VERSION" in \
      22.04) codecs='libtiff5 libpng16-16 libcups2' ;; \
      24.04) codecs='libtiff6 libpng16-16t64 libcups2t64' ;; \
      *) exit 2 ;; \
    esac && \
    DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
      mono-runtime libmono-system-windows-forms4.0-cil libmono-system-design4.0-cil \
      libmono-system-numerics4.0-cil libmono-system-runtime-serialization4.0-cil \
      libmono-microsoft-csharp4.0-cil libmono-system-net-http4.0-cil \
      libmono-system-data4.0-cil libmono-system-xml-linq4.0-cil \
      libmono-posix4.0-cil \
      libpangocairo-1.0-0 libcairo2 libjpeg-turbo8 $codecs libgif7 libexif12 \
      libx11-6 libxtst6 libxinerama1 fonts-dejavu-core \
      xvfb xauth xdotool x11-utils libxres1 && \
    rm -rf /var/lib/apt/lists/*
ENV LANG=C.UTF-8
