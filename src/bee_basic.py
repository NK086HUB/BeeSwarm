#!/usr/bin/env python3
"""
BeeSwarm — прототип симуляции пчелиного роя.
Агентное моделирование: пчёлы ищут цветы, собирают нектар, возвращаются в улей.
"""

import numpy as np
import matplotlib.pyplot as plt
from matplotlib import animation
import random, math
from dataclasses import dataclass
from typing import List, Tuple

# ============================================================
# КОНФИГ
# ============================================================

@dataclass
class Config:
    bees: int = 60
    flowers: int = 30
    
    # движение
    speed: float = 4.0
    steer_strength: float = 0.12  # резкость поворота
    
    # сбор
    forage_radius: float = 100.0  # радиус поиска цветов
    nectar_per_tick: float = 3.0  # нектара за шаг на цветке
    max_nectar: float = 30.0      # полный "рюкзак"
    return_at: float = 15.0       # при таком количестве — домой
    ticks_on_flower: int = 4      # шагов на цветке чтобы собрать
    
    # улей
    hive_x: float = 250.0
    hive_y: float = 250.0
    
    # мир
    world_size: float = 500.0


# ============================================================
# ПЧЕЛА
# ============================================================


class Bee:
    def __init__(self, bid: int, cfg: Config):
        self.id = bid
        self.cfg = cfg
        
        # старт в улье
        self.x = cfg.hive_x + random.uniform(-10, 10)
        self.y = cfg.hive_y + random.uniform(-10, 10)
        self.angle = random.uniform(0, 2 * math.pi)
        
        self.nectar = 0.0
        self.returning = False
        self.target: Tuple[float, float] | None = None
        self.on_flower_ticks = 0
        
    def dist_to(self, x: float, y: float) -> float:
        return math.hypot(self.x - x, self.y - y)
    
    def reached_target(self, tol: float = 20.0) -> bool:
        return self.target is None or self.dist_to(*self.target) < tol
        if self.target is None:
            return True
        return self.dist_to(*self.target) < tol
    
    def steer_to(self, tx: float, ty: float):
        ta = math.atan2(ty - self.y, tx - self.x)
        d = ta - self.angle
        while d > math.pi:
            d -= 2 * math.pi
        while d < -math.pi:
            d += 2 * math.pi
        self.angle += d * self.cfg.steer_strength
        self.angle %= 2 * math.pi
        
    def move(self):
        self.x += math.cos(self.angle) * self.cfg.speed
        self.y += math.sin(self.angle) * self.cfg.speed
        # границы мира
        self.x = max(0, min(self.cfg.world_size, self.x))
        self.y = max(0, min(self.cfg.world_size, self.y))
        
    def pick_target(self, flowers: List[Tuple[float, float, float]]):
        nearby = [(fx, fy) for fx, fy, _ in flowers 
                  if self.dist_to(fx, fy) < self.cfg.forage_radius]
        if nearby:
            self.target = random.choice(nearby)
        else:
            # случайная точка
            self.target = (random.uniform(0, self.cfg.world_size),
                          random.uniform(0, self.cfg.world_size))
    
    def update(self, flowers: List[Tuple[float, float, float]]):
        if self.returning:
            # летим домой
            self.steer_to(self.cfg.hive_x, self.cfg.hive_y)
            self.move()
            if self.dist_to(self.cfg.hive_x, self.cfg.hive_y) < 15:
                self.nectar = 0
                self.returning = False
                self.target = None
        else:
            if self.target is None:
                self.pick_target(flowers)
            
            self.steer_to(*self.target)
            self.move()
            
            if self.reached_target():
                self.on_flower_ticks += 1
                # собираем нектар каждый tick на цветке
                self.nectar = min(self.nectar + self.cfg.nectar_per_tick,
                                  self.cfg.max_nectar)
                if self.nectar >= self.cfg.return_at:
                    self.returning = True
                    self.target = None
                    self.on_flower_ticks = 0
                # остаёмся на цветке (не меняем target)
                self.on_flower_ticks = 0


# ============================================================
# СИМУЛЯЦИЯ
# ============================================================


class Simulation:
    def __init__(self, cfg: Config | None = None):
        self.cfg = cfg or Config()
        
        # цветы — кластеры
        self.flowers = []
        for _ in range(self.cfg.flowers):
            cx = random.uniform(50, 450)
            cy = random.uniform(50, 450)
            for _ in range(random.randint(2, 5)):
                self.flowers.append((
                    cx + random.uniform(-12, 12),
                    cy + random.uniform(-12, 12),
                    random.uniform(10, 30)  # запас
                ))
        
        self.bees = [Bee(i, self.cfg) for i in range(self.cfg.bees)]
        self.time = 0
        self.history = []
        
    def step(self):
        for b in self.bees:
            b.update(self.flowers)
        self.time += 1
        
        if self.time % 50 == 0:
            nectar_total = sum(b.nectar for b in self.bees)
            returning = sum(1 for b in self.bees if b.returning)
            self.history.append(dict(
                time=self.time,
                nectar=nectar_total,
                returning=returning
            ))
        
    def run(self, steps: int = 2000):
        for _ in range(steps):
            self.step()
        return self

    def status(self):
        nectar_total = sum(b.nectar for b in self.bees)
        returning = sum(1 for b in self.bees if b.returning)
        foraging = sum(1 for b in self.bees if not b.returning)
        return dict(
            time=self.time,
            bees=len(self.bees),
            foraging=foraging,
            returning=returning,
            nectar=round(nectar_total, 1)
        )


# ============================================================
# ВИЗУАЛИЗАЦИЯ
# ============================================================


def animate(sim: Simulation, steps: int = 1000):
    fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(14, 6))
    
    ax1.set_xlim(0, 500)
    ax1.set_ylim(0, 500)
    ax1.set_aspect('equal')
    ax1.set_title(f'🐝 BeeSwarm (t=0)')
    
    # улей
    ax1.plot(sim.cfg.hive_x, sim.cfg.hive_y, 'r^', ms=15, zorder=5)
    
    # цветы
    fxs = [f[0] for f in sim.flowers]
    fys = [f[1] for f in sim.flowers]
    ax1.scatter(fxs, fys, c='gold', alpha=0.5, s=25, zorder=2)
    
    dots = ax1.scatter([b.x for b in sim.bees], [b.y for b in sim.bees],
                       c='orange', s=15, zorder=4)
    
    ax2.set_title('Активность')
    ax2.set_xlabel('Время')
    ax2.set_ylabel('Пчёлы')
    ret_line, = ax2.plot([], [], 'b-', label='возвращаются')
    nect_line, = ax2.plot([], [], 'g-', label='нектар (x10)')
    ax2.legend()
    
    ret_data, nect_data = [], []
    
    def update(frame):
        for _ in range(5):
            sim.step()
        
        s = sim.status()
        ret_data.append(s['returning'])
        nect_data.append(s['nectar'] / 10)
        
        if len(ret_data) > steps:
            ret_data.pop(0)
            nect_data.pop(0)
        
        colors = ['blue' if b.returning else 'orange' for b in sim.bees]
        dots.set_offsets(np.c_[[b.x for b in sim.bees],
                               [b.y for b in sim.bees]])
        dots.set_color(colors)
        
        ax1.set_title(f'🐝 BeeSwarm (t={sim.time})')
        
        x = list(range(len(ret_data)))
        ret_line.set_data(x, ret_data)
        nect_line.set_data(x, nect_data)
        ax2.relim()
        ax2.autoscale_view()
        
        return dots, ret_line, nect_line
    
    ani = animation.FuncAnimation(fig, update, frames=range(steps // 5),
                                   interval=40, blit=True)
    plt.tight_layout()
    plt.show()
    return ani


# ============================================================
# ЗАПУСК
# ============================================================


if __name__ == '__main__':
    sim = Simulation()
    sim.run(1000)
    s = sim.status()
    print(f"🐝 BeeSwarm — {sim.time} шагов")
    print(f"   Пчёл: {s['bees']}")
    print(f"   Собирают: {s['foraging']}")
    print(f"   Возвращаются: {s['returning']}")
    print(f"   Всего нектара: {s['nectar']}")
    print()
    print("Для анимации: python3 -c 'from bee_basic import Simulation, animate; animate(Simulation())'")
