from bee_basic import Simulation

sim = Simulation()
sim.run(5000)
s = sim.status()
print(f"Шагов: {s['time']}, сбор: {s['foraging']}, возврат: {s['returning']}, нектар: {s['nectar']}")

b = sim.bees[0]
print(f"Пчела 0: нектар={b.nectar:.1f}, возврат={b.returning}, цветок_тики={b.on_flower_ticks}, цель={b.target}")
