import matplotlib.pyplot as plt
from matplotlib import animation
import pandas as pd

### UNCOMMENT to Update the plot in real time ###
# count = 0

# x = []
# y = []

# def Draw(i):
#     data = pd.read_csv("C:\\Users\\fergr\\Documents\\Universidad\\3er Año\\_SAXION\\T1_Advanced Tools\\SmartBoats\\boats.csv")
#     global count
#     count += 1
#     x.append(count)
#     y.append(data['Points'][count])

#     plt.cla()
#     plt.scatter(x,y)
#     plt.plot(x,y)


# anim = animation.FuncAnimation(plt.gcf(), Draw, interval=5000)

# plt.show()

### UNCOMMENT to Directly plot the whole data ###

data = pd.read_csv("C:\\Users\\fergr\\Documents\\Universidad\\3er Año\\_SAXION\\T1_Advanced Tools\\SmartBoats\\piratesParentsFitness.csv")
x = []
y = []

for a in range(1, data["Points"].count()):
    x.append(a)
    y.append(data["Points"][a])

plt.scatter(x,y)
plt.plot(x,y)
plt.show()
