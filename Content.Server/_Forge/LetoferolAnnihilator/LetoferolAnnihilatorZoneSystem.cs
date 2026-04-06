using Robust.Server.Player;
using Robust.Shared.Timing;
using Content.Shared._Forge.LetoferolAnnihilator;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.NPC.Systems;

namespace Content.Server._Forge.LetoferolAnnihilator
{
    public sealed class LetoferolAnnihilatorZoneSystem : EntitySystem
    {
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly NpcFactionSystem _factionSystem = default!;
        [Dependency] private readonly DamageableSystem _damageable = default!;

        private const string ZonePrototype = "AnnihilatorZone";

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var genQuery = AllEntityQuery<LetoferolAnnihilatorZoneComponent>();
            while (genQuery.MoveNext(out var genUid, out var component))
            {
                if (_gameTiming.CurTime < component.NextUpdate)
                    continue;

                UpdateDamageState(genUid, component);
            }
        }

        private void UpdateDamageState(EntityUid genUid, LetoferolAnnihilatorZoneComponent component)
        {
            var damage = component.Damage;
            if (damage == null || damage.Empty)
                return;

            var transform = Transform(genUid);
            var entities = _lookup.GetEntitiesInRange(transform.Coordinates, component.Radius);

            foreach (var entity in entities)
            {
                if (!_factionSystem.IsMember(entity, component.Target))
                    continue;

                _damageable.TryChangeDamage(entity, damage, ignoreResistances: false);
            }

            component.NextUpdate = _gameTiming.CurTime + component.UpdateInterval;
        }
    }
}
