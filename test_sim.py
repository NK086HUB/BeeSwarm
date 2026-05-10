import sys
sys.path.insert(0, '/home/lira/BeeSwarm/src')
from bee_basic import BeeSwarmSimulation

sim = BeeSwarmSimulation(20, 10)
for step in range(500):
    sim.update()
    if step % 100 == 0:
        s = sim.get_state()
        b = sim.bees[0]
        if b.target_x:
            d = ((b.x - b.target_x)**2 + (b.y - b.target_y)**2)**0.5
        else:
            d = -1
        print(f"Шаг {step:3d}: сбор={s['foraging']} возвр={s['returning']} разв={s['scouts']} dist={d:.0f} nect={b.nectar:.1f} sc={b.is_scout} col={b.collecting_time}")
