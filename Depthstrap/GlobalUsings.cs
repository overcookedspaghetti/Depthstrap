// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Globalization;
global using System.IO;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using System.Linq;
global using System.Net;
global using System.Net.Http;
global using System.Threading;
global using System.Threading.Tasks;

global using Depthstrap.Enums;
global using Depthstrap.Enums.Messagebox;
global using Depthstrap.Enums.BootstrapperDialogs;
global using Depthstrap.Exceptions;
global using Depthstrap.Extensions;
global using Depthstrap.Models;
global using Depthstrap.Models.APIs;
global using Depthstrap.Models.APIs.Config;
global using Depthstrap.Models.APIs.GitHub;
global using Depthstrap.Models.APIs.Roblox;
global using Depthstrap.Models.APIs.RoValra;
global using Depthstrap.Models.APIs.WebView2;
global using Depthstrap.Models.Attributes;
global using Depthstrap.Models.BloxstrapRPC;
global using Depthstrap.Models.Entities;
global using Depthstrap.Models.DepthstrapStudioRPC;
global using Depthstrap.Models.Manifest;
global using Depthstrap.Models.Persistable;
global using Depthstrap.Models.SettingTasks;
global using Depthstrap.Models.SettingTasks.Base;
global using Depthstrap.Resources;
global using Depthstrap.UI;
global using Depthstrap.Utility;
