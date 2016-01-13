DESTDIR?=/usr/local
DESTDIR2=$(shell realpath $(DESTDIR))
MONO_PATH?=/usr/bin

EX_NUGET:=nuget/bin/nuget

XBUILD?=$(MONO_PATH)/xbuild
MONO?=$(MONO_PATH)/mono
GIT?=$(shell which git)

NUGET?=$(EX_NUGET)

all: binary ;

binary: nuget-packages-restore 
	$(XBUILD) Twift.sln /p:Configuration=Release

# External tools

external-tools: nuget ;

nuget: $(NUGET) ;

submodule:
	$(GIT) submodule update --init --recursive

$(EX_NUGET): submodule
	cd nuget && $(MAKE)

# NuGet

nuget-packages-restore: external-tools
	[ -d packages ] || \
	    $(NUGET) restore -ConfigFile Twift/packages.config -PackagesDirectory packages ; \

# Install

install: binary 
	mkdir -p $(DESTDIR2)/lib/twift $(DESTDIR2)/bin
	cp Twift/bin/Release/* $(DESTDIR2)/lib/twift/
	echo "#!/bin/sh" > $(DESTDIR2)/bin/twift
	echo "mono $(DESTDIR2)/lib/twift/twift.exe \$$*" >> $(DESTDIR2)/bin/twift
	chmod +x $(DESTDIR2)/bin/twift

# Clean

clean:
	$(RM) -rf Twift/obj

