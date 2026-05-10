from bee_basic import Simulation, Config

cfg = Config()
sim = Simulation(cfg)

# трассировка одной пчелы
b = sim.bees[0]
print("Трассировка пчелы 0:")
for step in range(2000):
    sim.step()
    if step % 200 == 0 or (step < 50 and step % 10 == 0):
        nectar = b.nectar
        returning = b.returning
        if b.target:
            d = ((b.x - b.target[0])**2 + (b.y - b.target[1])**2)**0.5
            t = f"target dist={d:.0f}"
        else:
            t = "no target"
        print(f"  t={step:4d} n={nectar:.0f} ret={returning} {t} x={b.x:.0f} y={b.y:.0f}")

# финальная статистика по всем
s = sim.status()
ret = sum(1 for b in sim.bees if b.returning)
high = sum(1 for b in sim.bees if b.nectar >= cfg.return_at)
print(f"\nФинально: сбор={s['foraging']} возврат={s['returning']} нектар={s['nectar']}")
print(f"Пчёл с нектаром >= {cfg.return_at}: {high}")

# сколько раз пчёлы возвращались в улей?
runs = sum(1 for b in sim.bees if b.returning or b.nectar >= cfg.return_at)
print(f"Готовы к возврату: {runs}")
