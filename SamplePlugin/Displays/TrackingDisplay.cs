using Dalamud.Game.Gui.NamePlate;
using Dalamud.Game.Text.SeStringHandling;
using SamplePlugin.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SamplePlugin.Displays;

public sealed class TrackingDisplay : IDisposable
{
    private static readonly SeString GotIcon = new SeStringBuilder()
        .AddIcon(BitmapFontIcon.RedStar)
        .Build();

    private static readonly SeString HiddenIcon = new SeStringBuilder()
        .AddIcon(BitmapFontIcon.BlueStar)
        .Build();

    private static readonly long CACHE_TTL_MS = 5 * 60 * 1000;

    private readonly EchoVault service;

    private readonly ConcurrentDictionary<Character, Entry> cache = new();
    private readonly ConcurrentDictionary<Character, byte> inFlight = new();

    public TrackingDisplay(EchoVault service)
    {
        this.service = service;
        Plugin.NamePlateGui.OnNamePlateUpdate += OnNamePlateUpdate;
    }

    private void OnNamePlateUpdate(
        INamePlateUpdateContext context,
        IReadOnlyList<INamePlateUpdateHandler> handlers
    ) {
        try
        {
            foreach (var handler in handlers)
            {
                if (handler.PlayerCharacter is null)
                {
                    continue;
                }

                if (handler.PlayerCharacter.GameObjectId == Plugin.ObjectTable.LocalPlayer?.GameObjectId)
                {
                    continue;
                }

                var name = handler.PlayerCharacter.Name.TextValue;
                var world = handler.PlayerCharacter.HomeWorld.RowId;

                if (string.IsNullOrEmpty(name) || world == 0) {
                    continue;
                }

                var character = new Character(world, name);

                if (cache.TryGetValue(character, out var entry))
                {
                    if (entry.Result == EchoVault.ProfileStatus.EXIST)
                    {
                        handler.NameParts.TextWrap = (GotIcon, SeString.Empty);
                    }
                    else if (entry.Result == EchoVault.ProfileStatus.HIDDEN)
                    {
                        handler.NameParts.TextWrap = (HiddenIcon, SeString.Empty);
                    }

                    if (Environment.TickCount64 - entry.FetchedAt >= CACHE_TTL_MS && inFlight.TryAdd(character, 0))
                    {
                        Fetch(character);
                    }
                }
                else if (inFlight.TryAdd(character, 0))
                {
                    Fetch(character);
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "OnNamePlateUpdate threw");
        }
    }

    private async void Fetch(Character character)
    {
        try
        {
            var result = await service
                .HasCharacter(character.World, character.Name, Plugin.Configuration.apiKey)
                .ConfigureAwait(false);

            cache[character] = new Entry(result, Environment.TickCount64);

            await Plugin.Framework
                .RunOnFrameworkThread(() => Plugin.NamePlateGui.RequestRedraw())
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, $"{character}: fetch failed");
        }
        finally
        {
            inFlight.TryRemove(character, out _);
        }
    }

    public void Dispose()
    {
        Plugin.NamePlateGui.OnNamePlateUpdate -= OnNamePlateUpdate;

        cache.Clear();
        inFlight.Clear();
    }
}

internal record struct Character(uint World, string Name);
internal record struct Entry(EchoVault.ProfileStatus Result, long FetchedAt);
