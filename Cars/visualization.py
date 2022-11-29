import random
from mesa.visualization.modules import CanvasGrid
from mesa.visualization.ModularVisualization import ModularServer
from mesa.visualization.UserParam import Slider,NumberInput

from model import CarsModel,RowAgent,BuildingAgent,TrafficLightAgent,CarAgent

colors = {}
def agentPortrayal(agent):
    """
    Portrayal Method for canvas
    input: agent
    output: portrayal
    """
    #Defualt portrayal
    portrayal = {"Shape": "circle",
                 "Filled": "true",
                 "r": 0.5,
                 "Layer": 0,
                 "Color": "red",
                 "text": agent.unique_id,
                 "text_color": "black"}
    if isinstance(agent,RowAgent):
        portrayal["Shape"] = "rect"
        portrayal["w"] = 1
        portrayal["h"] = 1
        portrayal["Layer"] = 0
        portrayal["Color"] = "gray"
        portrayal["text"] = agent.direction
    elif isinstance(agent,BuildingAgent) and agent.isDestination():
        portrayal["Shape"] = "rect"
        portrayal["w"] = 1
        portrayal["h"] = 1
        portrayal["Layer"] = 0
        portrayal["Color"] = "blue"
        portrayal["text"] = "" 

    elif isinstance(agent,BuildingAgent):
        portrayal["Shape"] = "rect"
        portrayal["w"] = 1
        portrayal["h"] = 1
        portrayal["Layer"] = 0
        portrayal["Color"] = "black"
        portrayal["text"] = ""
    elif isinstance(agent,TrafficLightAgent):
        portrayal["Layer"] = 0
        portrayal["Color"] = agent.getStatusColor()
        portrayal["text"] = ""
    elif isinstance(agent,CarAgent):
        if agent.unique_id not in colors:
            colors[agent.unique_id] = random.choice(["red","green","blue","yellow"])
           
        portrayal["Shape"] = "rect"
        portrayal["w"] = 0.8
        portrayal["h"] = 0.6
        portrayal["Layer"] = 1
        portrayal["Color"] = colors[agent.unique_id]
        portrayal["text"] = ""
    return portrayal

def getDimensionsFromFile(filename):
    """
    Get dimensions of the grid from the file
    input: filename
    output: x,y
    """
    with open(filename) as f:
        data = f.read()
    data = data.split("\n")
    x = len(data[0])
    y = len(data)
    return x,y

width,height = getDimensionsFromFile("base2.txt")

modelParams = {"file": "base2.txt","width": width,"height": height}

grid = CanvasGrid(agentPortrayal, width, height, 500, 500)

server = ModularServer(CarsModel, [grid], "Cars Model", modelParams)

server.port = 8521 # The default
server.launch()