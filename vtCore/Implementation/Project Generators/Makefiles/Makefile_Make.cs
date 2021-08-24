﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using vtCore.Interfaces;
using System.Drawing;

namespace vtCore
{
    static internal class Makefile_Make
    {
        static public string generate(IProject project, LibManager libManager, SetupData setup)
        {
            var cfg = project.selectedConfiguration;
            if (!cfg.isOk) return "ERROR";
            var board = cfg.selectedBoard;
            var options = board.getAllOptions();

            var v = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;

            StringBuilder mf = new StringBuilder();
            mf.Append($"#******************************************************************************\n");
            mf.Append($"# Generated by VisualTeensy V{v.Major}.{v.Minor}.{v.Build} on {DateTime.Now.ToShortDateString()} at {DateTime.Now.ToShortTimeString()}\n");        
            mf.Append("#\n");
            mf.Append($"# {"Board",-18} {board.name}\n");
            foreach (var o in board.optionSets)
            {
                mf.Append($"# {o.name,-18} {o.selectedOption?.name}\n");
            }
            mf.Append("#\n");
          //  mf.Append($"# {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}\n");
            mf.Append($"# https://github.com/luni64/VisualTeensy\n");
            mf.Append("#******************************************************************************\n");

            mf.Append($"SHELL            := cmd.exe\nexport SHELL\n\n");
            mf.Append($"TARGET_NAME      := {project.cleanName}\n");
            mf.Append(makeEntry("BOARD_ID         := ", "build.board", options) + "\n\n");
            mf.Append(makeEntry("MCU              := ", "build.mcu", options) + "\n\n");

            mf.Append($"LIBS_SHARED_BASE := {Helpers.getShortPath(libManager.sharedRepository?.repoPath)}\n");
            mf.Append($"LIBS_SHARED      := ");
            foreach (var lib in cfg.sharedLibs)
            {
                mf.Append($"{lib.sourceFolderName ?? "ERROR"} ");
            }
            mf.Append("\n\n");

            mf.Append($"LIBS_LOCAL_BASE  := lib\n");
            mf.Append($"LIBS_LOCAL       := ");
            foreach (var lib in cfg.localLibs)
            {
                mf.Append($"{lib.targetFolder} ");
            }
            mf.Append("\n\n");

            //mf.Append($"CORE_BASE        := {Helpers.getShortPath(Path.Combine(setup.arduinoCoreBase ?? "Error", "cores", cfg.selectedBoard.core))}\n");

            if (cfg.setupType == SetupTypes.quick)
            {
                mf.Append($"CORE_BASE        := {Helpers.getShortPath(Path.Combine(setup.arduinoCoreBase ?? "Error", "cores", cfg.selectedBoard.core))}\n");
                mf.Append($"GCC_BASE         := {cfg.compiler}\n");
                mf.Append($"UPL_PJRC_B       := {Helpers.getShortPath(setup.arduinoTools)}\n");
            }
            else
            {
                switch (cfg.coreStrategy)
                {
                    case LibStrategy.link:
                        mf.Append($"CORE_BASE        := {Helpers.getShortPath(Path.Combine(cfg.coreBase.path , "cores", cfg.selectedBoard.core))}\n");
                        break;

                    case LibStrategy.copy:
                    case LibStrategy.clone:
                        mf.Append($"CORE_BASE        := {Path.Combine("cores", cfg.selectedBoard.core)}\n");
                        break;

                }
                mf.Append($"GCC_BASE         := {cfg.compiler}\n");
                               
                if (!String.IsNullOrWhiteSpace(setup.uplPjrcBase.path)) mf.Append($"UPL_PJRC_B       := {setup.uplPjrcBase.shortPath}\n");
                //mf.Append($"UPL_PJRC_B       := {setup.uplPjrcBase.shortPath}\n");
            }
            if (!String.IsNullOrWhiteSpace(setup.uplTyBase.path)) mf.Append($"UPL_TYCMD_B      := {setup.uplTyBase.shortPath}\n");
            if (!String.IsNullOrWhiteSpace(setup.uplJLinkBase.path)) mf.Append($"UPL_JLINK_B      := {setup.uplJLinkBase.shortPath}\n");
            if (project.debugSupport != DebugSupport.none && !String.IsNullOrWhiteSpace(setup.uplCLIBase.path)) mf.Append($"UPL_CLICMD_B     := {setup.uplCLIBase.shortPath}\n");



            mf.Append("\n#******************************************************************************\n");
            mf.Append("# Flags and Defines\n");
            mf.Append("#******************************************************************************\n");

            if (makeEntry("dummy", "build.flags.ld", options).Contains("TIME_SYM"))
            {
                mf.Append("TIME_SYM    := $(shell powershell [int][double]::Parse((Get-Date -UFormat %s)))" + "\n");
            }

            mf.Append("\n");
            mf.Append(makeEntry("FLAGS_CPU   := ", "build.flags.cpu", options) + "\n");
            mf.Append(makeEntry("FLAGS_OPT   := ", "build.flags.optimize", options) + "\n");
            mf.Append(makeEntry("FLAGS_COM   := ", "build.flags.common", options) + makeEntry(" ", "build.flags.dep", options) + "\n");
            mf.Append(makeEntry("FLAGS_LSP   := ", "build.flags.ldspecs", options) + "\n");

            mf.Append("\n");
            mf.Append(makeEntry("FLAGS_CPP   := ", "build.flags.cpp", options) + "\n");
            mf.Append(makeEntry("FLAGS_C     := ", "build.flags.c", options) + "\n");
            mf.Append(makeEntry("FLAGS_S     := ", "build.flags.S", options) + "\n");
            mf.Append(makeEntry("FLAGS_LD    := ", "build.flags.ld", options) + "\n");

            mf.Append("\n");
            mf.Append(makeEntry("LIBS        := ", "build.flags.libs", options) + "\n");

            mf.Append("\n");
            mf.Append(makeEntry("DEFINES     := ", "build.flags.defs", options) + makeEntry(" -DARDUINO_", "build.board", options) + " -DARDUINO=10813\n");
            mf.Append("DEFINES     += ");
            mf.Append(makeEntry("-DF_CPU=", "build.fcpu", options) + " " + makeEntry("-D", "build.usbtype", options) + " " + makeEntry("-DLAYOUT_", "build.keylayout", options) + "\n");

            mf.Append($"\n");
            mf.Append("CPP_FLAGS   := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_COM) $(DEFINES) $(FLAGS_CPP)\n");
            mf.Append("C_FLAGS     := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_COM) $(DEFINES) $(FLAGS_C)\n");
            mf.Append("S_FLAGS     := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_COM) $(DEFINES) $(FLAGS_S)\n");
            mf.Append("LD_FLAGS    := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_LSP) $(FLAGS_LD)\n");
            mf.Append("AR_FLAGS    := rcs\n");
            mf.Append("NM_FLAGS    := --numeric-sort --defined-only --demangle --print-size\n");

            if (cfg.setupType == SetupTypes.expert && !String.IsNullOrWhiteSpace(cfg.makefileExtension))
            {
                mf.Append("\n");
                mf.Append(cfg.makefileExtension);
                mf.Append("\n");
            }

            mf.Append("\n#******************************************************************************\n");
            mf.Append("# Colors\n");
            mf.Append("#******************************************************************************\n");
            if (setup.isColoredOutput)
            {
                mf.Append($"COL_CORE    := {colEsc(setup.colorCore)}\n");
                mf.Append($"COL_LIB     := {colEsc(setup.colorUserLib)}\n");
                mf.Append($"COL_SRC     := {colEsc(setup.colorUserSrc)}\n");
                mf.Append($"COL_LINK    := {colEsc(setup.colorLink)}\n");
                mf.Append($"COL_ERR     := {colEsc(setup.colorErr)}\n");
                mf.Append($"COL_OK      := {colEsc(setup.colorOk)}\n");
                mf.Append($"COL_RESET   := {colReset}\n");
            }
            else
            {
                mf.Append($"COL_CORE    := {colReset}\n");
                mf.Append($"COL_Lib     := {colReset}\n");
                mf.Append($"COL_SRC     := {colReset}\n");
                mf.Append($"COL_LINK    := {colReset}\n");
                mf.Append($"COL_ERR     := {colReset}\n");
                mf.Append($"COL_OK      := {colReset}\n");
                mf.Append($"COL_RESET   := {colReset}\n");
            }

            mf.Append("\n");
            mf.Append("#******************************************************************************\n");
            mf.Append("# Folders and Files\n");
            mf.Append("#******************************************************************************\n");
            //if (cfg.setupType == SetupTypes.expert && project.buildSystem == BuildSystem.makefile && project.useInoFiles)
            //{
            //    mf.Append("USR_SRC         := .\n");
            //}
            //else
            //{
            mf.Append("USR_SRC         := src\n");
            //}
            mf.Append("LIB_SRC         := lib\n");
            mf.Append("CORE_SRC        := $(CORE_BASE)\n\n");

            mf.Append("BIN             := .vsteensy/build\n");
            mf.Append("USR_BIN         := $(BIN)/src\n");
            mf.Append("CORE_BIN        := $(BIN)/core\n");
            mf.Append("LIB_BIN         := $(BIN)/lib\n");
            mf.Append("CORE_LIB        := $(BIN)/core.a\n");
            mf.Append("TARGET_HEX      := $(BIN)/$(TARGET_NAME).hex\n");
            mf.Append("TARGET_ELF      := $(BIN)/$(TARGET_NAME).elf\n");
            mf.Append("TARGET_LST      := $(BIN)/$(TARGET_NAME).lst\n");
            mf.Append("TARGET_SYM      := $(BIN)/$(TARGET_NAME).sym\n");

            mf.Append("\n");
            mf.Append("#******************************************************************************\n");
            mf.Append("# BINARIES\n");
            mf.Append("#******************************************************************************\n");
            mf.Append("CC              := $(GCC_BASE)/arm-none-eabi-gcc\n");
            mf.Append("CXX             := $(GCC_BASE)/arm-none-eabi-g++\n");
            mf.Append("AR              := $(GCC_BASE)/arm-none-eabi-gcc-ar\n");
            mf.Append("NM              := $(GCC_BASE)/arm-none-eabi-gcc-nm\n");
            mf.Append("SIZE            := $(GCC_BASE)/arm-none-eabi-size\n");
            mf.Append("OBJDUMP         := $(GCC_BASE)/arm-none-eabi-objdump\n");
            mf.Append("OBJCOPY         := $(GCC_BASE)/arm-none-eabi-objcopy\n");
            mf.Append("UPL_PJRC        := \"$(UPL_PJRC_B)/teensy_post_compile\" -test -file=$(TARGET_NAME) -path=$(BIN) -tools=\"$(UPL_PJRC_B)\" -board=$(BOARD_ID) -reboot\n");
            mf.Append("UPL_TYCMD       := $(UPL_TYCMD_B)/tyCommanderC upload $(TARGET_HEX) --autostart --wait --multi\n");
            mf.Append("UPL_CLICMD      := $(UPL_CLICMD_B)/teensy_loader_cli -mmcu=$(MCU) -v $(TARGET_HEX)\n");
            mf.Append("UPL_JLINK       := $(UPL_JLINK_B)/jlink -commanderscript .vsteensy/flash.jlink\n");

            mf.Append("\n");
            mf.Append("#******************************************************************************\n");
            mf.Append("# Source and Include Files\n");
            mf.Append("#******************************************************************************\n");
            mf.Append("# Recursively create list of source and object files in USR_SRC and CORE_SRC\n");
            mf.Append("# and corresponding subdirectories.\n");
            mf.Append("# The function rwildcard is taken from http://stackoverflow.com/a/12959694)\n");

            mf.Append("\n");
            mf.Append("rwildcard =$(wildcard $1$2) $(foreach d,$(wildcard $1*),$(call rwildcard,$d/,$2))\n");

            mf.Append("\n");
            mf.Append("#User Sources -----------------------------------------------------------------\n");
            //          if (cfg.setupType == SetupTypes.expert && project.buildSystem == BuildSystem.makefile && project.useInoFiles) mf.Append("USR_INO_FILE    := $(USR_SRC)/$(TARGET_NAME).ino\n");
            mf.Append("USR_C_FILES     := $(call rwildcard,$(USR_SRC)/,*.c)\n");
            mf.Append("USR_CPP_FILES   := $(call rwildcard,$(USR_SRC)/,*.cpp)\n");
            mf.Append("USR_S_FILES     := $(call rwildcard,$(USR_SRC)/,*.S)\n");
            mf.Append("USR_OBJ         := $(USR_S_FILES:$(USR_SRC)/%.S=$(USR_BIN)/%.s.o) $(USR_C_FILES:$(USR_SRC)/%.c=$(USR_BIN)/%.c.o) $(USR_CPP_FILES:$(USR_SRC)/%.cpp=$(USR_BIN)/%.cpp.o)\n");
            //          if (cfg.setupType == SetupTypes.expert && project.buildSystem == BuildSystem.makefile && project.useInoFiles) mf.Append("USR_OBJ         += $(USR_INO_FILE:$(USR_SRC)/%.ino=$(USR_BIN)/%.o)\n");

            mf.Append("\n");
            mf.Append("# Core library sources --------------------------------------------------------\n");
            mf.Append("CORE_CPP_FILES  := $(call rwildcard,$(CORE_SRC)/,*.cpp)\n");
            mf.Append("CORE_C_FILES    := $(call rwildcard,$(CORE_SRC)/,*.c)\n");
            mf.Append("CORE_S_FILES    := $(call rwildcard,$(CORE_SRC)/,*.S)\n");
            mf.Append("CORE_OBJ        := $(CORE_S_FILES:$(CORE_SRC)/%.S=$(CORE_BIN)/%.s.o) $(CORE_C_FILES:$(CORE_SRC)/%.c=$(CORE_BIN)/%.c.o) $(CORE_CPP_FILES:$(CORE_SRC)/%.cpp=$(CORE_BIN)/%.cpp.o)\n");

            mf.Append("\n");
            mf.Append("# User library sources (see https://github.com/arduino/arduino/wiki/arduino-ide-1.5:-library-specification)\n");
            mf.Append("LIB_DIRS_SHARED := $(foreach d, $(LIBS_SHARED), $(LIBS_SHARED_BASE)/$d/ $(LIBS_SHARED_BASE)/$d/utility/)      # base and /utility\n");
            mf.Append("LIB_DIRS_SHARED += $(foreach d, $(LIBS_SHARED), $(LIBS_SHARED_BASE)/$d/src/ $(dir $(call rwildcard,$(LIBS_SHARED_BASE)/$d/src/,*/.)))                          # src and all subdirs of base\n");

            mf.Append("\n");
            mf.Append("LIB_DIRS_LOCAL  := $(foreach d, $(LIBS_LOCAL), $(LIBS_LOCAL_BASE)/$d/ $(LIBS_LOCAL_BASE)/$d/utility/ )        # base and /utility\n");
            mf.Append("LIB_DIRS_LOCAL  += $(foreach d, $(LIBS_LOCAL), $(LIBS_LOCAL_BASE)/$d/src/ $(dir $(call rwildcard,$(LIBS_LOCAL_BASE)/$d/src/,*/.)))                          # src and all subdirs of base\n");

            mf.Append("\n");
            mf.Append("LIB_CPP_SHARED  := $(foreach d, $(LIB_DIRS_SHARED),$(call wildcard,$d*.cpp))\n");
            mf.Append("LIB_C_SHARED    := $(foreach d, $(LIB_DIRS_SHARED),$(call wildcard,$d*.c))\n");
            mf.Append("LIB_S_SHARED    := $(foreach d, $(LIB_DIRS_SHARED),$(call wildcard,$d*.S))\n");

            mf.Append("\n");
            mf.Append("LIB_CPP_LOCAL   := $(foreach d, $(LIB_DIRS_LOCAL),$(call wildcard,$d/*.cpp))\n");
            mf.Append("LIB_C_LOCAL     := $(foreach d, $(LIB_DIRS_LOCAL),$(call wildcard,$d/*.c))\n");
            mf.Append("LIB_S_LOCAL     := $(foreach d, $(LIB_DIRS_LOCAL),$(call wildcard,$d/*.S))\n");

            mf.Append("\n");
            mf.Append("LIB_OBJ         := $(LIB_CPP_SHARED:$(LIBS_SHARED_BASE)/%.cpp=$(LIB_BIN)/%.cpp.o)  $(LIB_CPP_LOCAL:$(LIBS_LOCAL_BASE)/%.cpp=$(LIB_BIN)/%.cpp.o)\n");
            mf.Append("LIB_OBJ         += $(LIB_C_SHARED:$(LIBS_SHARED_BASE)/%.c=$(LIB_BIN)/%.c.o)  $(LIB_C_LOCAL:$(LIBS_LOCAL_BASE)/%.c=$(LIB_BIN)/%.c.o)\n");
            mf.Append("LIB_OBJ         += $(LIB_S_SHARED:$(LIBS_SHARED_BASE)/%.S=$(LIB_BIN)/%.s.o)  $(LIB_S_LOCAL:$(LIBS_LOCAL_BASE)/%.S=$(LIB_BIN)/%.s.o)\n");

            mf.Append("\n");
            mf.Append("# Includes -------------------------------------------------------------\n");
            mf.Append("INCLUDE         := -I./$(USR_SRC) -I$(CORE_SRC)\n");
            mf.Append("INCLUDE         += $(foreach d, $(LIB_DIRS_SHARED), -I$d)\n");
            mf.Append("INCLUDE         += $(foreach d, $(LIB_DIRS_LOCAL), -I$d)\n");

            mf.Append("\n");
            mf.Append("# Generate directories --------------------------------------------------------\n");
            mf.Append("DIRECTORIES     :=  $(sort $(dir $(CORE_OBJ) $(USR_OBJ) $(LIB_OBJ)))\n");
            mf.Append("generateDirs    := $(foreach d, $(DIRECTORIES), $(shell if not exist \"$d\" mkdir \"$d\"))\n");

            mf.Append("\n");
            mf.Append("#$(info dirs: $(DIRECTORIES))\n");
            //mf.Append("$(info$(COL_RESET))\n");

            mf.Append("\n");
            mf.Append("#******************************************************************************\n");
            mf.Append("# Rules:\n");
            mf.Append("#******************************************************************************\n");

            mf.Append("\n");
            mf.Append(".PHONY: directories all rebuild upload uploadTy uploadCLI clean cleanUser cleanCore\n");

            mf.Append("\n");
            mf.Append("all:  $(TARGET_LST) $(TARGET_SYM) $(TARGET_HEX)\n");

            mf.Append("\n");
            mf.Append("rebuild: cleanUser all\n");

            mf.Append("\n");
            mf.Append("clean: cleanUser cleanCore cleanLib\n");
            mf.Append("\t@echo $(COL_OK)cleaning done$(COL_RESET)\n");

            mf.Append("\n");
            mf.Append("upload: all\n");
            mf.Append("\t@$(UPL_PJRC)\n");

            mf.Append("\n");
            mf.Append("uploadTy: all\n");
            mf.Append("\t@$(UPL_TYCMD)\n");

            mf.Append("\n");
            mf.Append("uploadCLI: all\n");
            mf.Append("\t@$(UPL_CLICMD)\n");

            mf.Append("\n");
            mf.Append("uploadJLink: all\n");
            mf.Append("\t@$(UPL_JLINK)\n");

            mf.Append("\n");
            mf.Append("# Core library ----------------------------------------------------------------\n");
            mf.Append("$(CORE_BIN)/%.s.o: $(CORE_SRC)/%.S\n");
            mf.Append("\t@echo $(COL_CORE)CORE [ASM] $(notdir $<) $(COL_ERR)\n");
            mf.Append("\t@\"$(CC)\" $(S_FLAGS) $(INCLUDE) -o $@ -c $<\n");

            mf.Append("\n");
            mf.Append("$(CORE_BIN)/%.c.o: $(CORE_SRC)/%.c\n");
            mf.Append("\t@echo $(COL_CORE)CORE [CC]  $(notdir $<) $(COL_ERR)\n");
            mf.Append("\t@\"$(CC)\" $(C_FLAGS) $(INCLUDE) -o $@ -c $<\n");

            mf.Append("\n");
            mf.Append("$(CORE_BIN)/%.cpp.o: $(CORE_SRC)/%.cpp\n");
            mf.Append("\t@echo $(COL_CORE)CORE [CPP] $(notdir $<) $(COL_ERR)\n");
            mf.Append("\t@\"$(CXX)\" $(CPP_FLAGS) $(INCLUDE) -o $@ -c $<\n");

            mf.Append("\n");
            mf.Append("$(CORE_LIB) : $(CORE_OBJ)\n");
            mf.Append("\t@echo $(COL_LINK)CORE [AR] $@ $(COL_ERR)\n");
            mf.Append("\t@$(AR) $(AR_FLAGS) $@ $^\n");
            mf.Append("\t@echo $(COL_OK)Teensy core built successfully &&echo.\n");

            mf.Append("\n");
            mf.Append("# Shared Libraries ------------------------------------------------------------\n");
            mf.Append("$(LIB_BIN)/%.s.o: $(LIBS_SHARED_BASE)/%.S\n");
            mf.Append("\t@echo $(COL_LIB)LIB [ASM] $(notdir $<) $(COL_ERR)\n");
            mf.Append("\t@\"$(CC)\" $(S_FLAGS) $(INCLUDE) -o $@ -c $<\n");

            mf.Append("\n");
            mf.Append("$(LIB_BIN)/%.cpp.o: $(LIBS_SHARED_BASE)/%.cpp\n");
            mf.Append("\t@echo $(COL_LIB)LIB [CPP] $(notdir $<) $(COL_ERR)\n");
            mf.Append("\t@\"$(CXX)\" $(CPP_FLAGS) $(INCLUDE) -o $@ -c $<\n");

            mf.Append("\n");
            mf.Append("$(LIB_BIN)/%.c.o: $(LIBS_SHARED_BASE)/%.c\n");
            mf.Append("\t@echo $(COL_LIB)LIB [CC]  $(notdir $<) $(COL_ERR)\n");
            mf.Append("\t@\"$(CC)\" $(C_FLAGS) $(INCLUDE) -o $@ -c $<\n");

            mf.Append("\n");
            mf.Append("# Local Libraries -------------------------------------------------------------\n");
            mf.Append("$(LIB_BIN)/%.s.o: $(LIBS_LOCAL_BASE)/%.S\n");
            mf.Append("\t@echo $(COL_LIB)LIB [ASM] $(notdir $<) $(COL_ERR)\n");
            mf.Append("\t@\"$(CC)\" $(S_FLAGS) $(INCLUDE) -o $@ -c $<\n");

            mf.Append("\n");
            mf.Append("$(LIB_BIN)/%.cpp.o: $(LIBS_LOCAL_BASE)/%.cpp\n");
            mf.Append("\t@echo $(COL_LIB)LIB [CPP] $(notdir $<) $(COL_ERR)\n");
            mf.Append("\t@\"$(CXX)\" $(CPP_FLAGS) $(INCLUDE) -o $@ -c $<\n");

            mf.Append("\n");
            mf.Append("$(LIB_BIN)/%.c.o: $(LIBS_LOCAL_BASE)/%.c\n");
            mf.Append("\t@echo $(COL_LIB)LIB [CC]  $(notdir $<) $(COL_ERR)\n");
            mf.Append("\t@\"$(CC)\" $(C_FLAGS) $(INCLUDE) -o $@ -c $<\n");

            mf.Append("\n");
            mf.Append("# Handle user sources ---------------------------------------------------------\n");
            //if (cfg.setupType == SetupTypes.expert && project.buildSystem == BuildSystem.makefile && project.useInoFiles)
            //{
            //    mf.Append("$(USR_BIN)/%.o: $(USR_SRC)/%.ino\n");
            //    mf.Append("\t@echo $(COL_SRC)USER [INO] $< $(COL_ERR)\n");
            //    mf.Append("\t@\"$(CC)\" $(CPP_FLAGS) $(INCLUDE) -include $(CORE_BASE)/Arduino.h -x c++ -o \"$@\" -c $< -x none\n");
            //    mf.Append("\n");
            //}

            mf.Append("$(USR_BIN)/%.s.o: $(USR_SRC)/%.S\n");
            mf.Append("\t@echo $(COL_SRC)USER [ASM] $< $(COL_ERR)\n");
            mf.Append("\t@\"$(CC)\" $(S_FLAGS) $(INCLUDE) -o \"$@\" -c $<\n");

            mf.Append("\n");
            mf.Append("$(USR_BIN)/%.c.o: $(USR_SRC)/%.c\n");
            mf.Append("\t@echo $(COL_SRC)USER [CC]  $(notdir $<) $(COL_ERR)\n");
            mf.Append("\t@\"$(CC)\" $(C_FLAGS) $(INCLUDE) -o \"$@\" -c $<\n");

            mf.Append("\n");
            mf.Append("$(USR_BIN)/%.cpp.o: $(USR_SRC)/%.cpp\n");
            mf.Append("\t@echo $(COL_SRC)USER [CPP] $(notdir $<) $(COL_ERR)\n");
            mf.Append("\t@\"$(CXX)\" $(CPP_FLAGS) $(INCLUDE) -o \"$@\" -c $<\n");

            mf.Append("\n");
            mf.Append("# Linking ---------------------------------------------------------------------\n");
            mf.Append("$(TARGET_ELF): $(CORE_LIB) $(LIB_OBJ) $(USR_OBJ)\n");
            mf.Append("\t@echo $(COL_LINK)\n");
            mf.Append("\t@echo [LD]  $@ $(COL_ERR)\n");
            mf.Append("\t@$(CC) $(LD_FLAGS) -o \"$@\" $(USR_OBJ) $(LIB_OBJ) $(CORE_LIB) $(LIBS)\n");
            mf.Append("\t@echo $(COL_OK)User code built and linked to libraries &&echo.\n");

            mf.Append("\n");
            mf.Append("%.lst: %.elf\n");
            mf.Append("\t@echo [LST] $@\n");
            mf.Append("\t@$(OBJDUMP) -d -S --demangle --no-show-raw-insn \"$<\" > \"$@\"\n");
            mf.Append("\t@echo $(COL_OK)Sucessfully built project$(COL_RESET) &&echo.\n");

            mf.Append("\n");
            mf.Append("%.sym: %.elf\n");
            mf.Append("\t@echo [SYM] $@\n");
            mf.Append("\t@$(NM) $(NM_FLAGS) \"$<\" > \"$@\"\n");

            mf.Append("\n");
            mf.Append("%.hex: %.elf\n");
            mf.Append("\t@echo $(COL_LINK)[HEX] $@\n");
            mf.Append("\t@$(OBJCOPY) -O ihex -R.eeprom \"$<\" \"$@\"\n");

            mf.Append("\n");
            mf.Append("# Cleaning --------------------------------------------------------------------\n");
            mf.Append("cleanUser:\n");
            mf.Append("\t@echo $(COL_LINK)Cleaning user binaries...$(COL_RESET)\n");
            mf.Append("\t@if exist $(USR_BIN) rd /s/q \"$(USR_BIN)\"\n");
            mf.Append("\t@if exist \"$(TARGET_LST)\" del $(subst /,\\,$(TARGET_LST))\n");

            mf.Append("\n");
            mf.Append("cleanCore:\n");
            mf.Append("\t@echo $(COL_LINK)Cleaning core binaries...$(COL_RESET)\n");
            mf.Append("\t@if exist $(CORE_BIN) rd /s/q \"$(CORE_BIN)\"\n");
            mf.Append("\t@if exist $(CORE_LIB) del  $(subst /,\\,$(CORE_LIB))\n");

            mf.Append("\n");
            mf.Append("cleanLib:\n");
            mf.Append("\t@echo $(COL_LINK)Cleaning user library binaries...$(COL_RESET)\n");
            mf.Append("\t@if exist $(LIB_BIN) rd /s/q \"$(LIB_BIN)\"\n");

            mf.Append("\n");
            mf.Append("# compiler generated dependency info ------------------------------------------\n");
            mf.Append("-include $(CORE_OBJ:.o=.d)\n");
            mf.Append("-include $(USR_OBJ:.o=.d)\n");
            mf.Append("-include $(LIB_OBJ:.o=.d)");

            return mf.ToString();
        }

        private static string makeEntry(String txt, String key, Dictionary<String, String> options)
        {
            return options.ContainsKey(key) ? $"{txt}{options[key]}" : "";
        }

        private static string colEsc(Color c)
        {
            return $"{(char)27}[38;2;{c.R};{c.G};{c.B}m";
        }

        private static readonly string colReset = $"{(char)27}[0m";
    }
}

