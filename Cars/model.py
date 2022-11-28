import random
from mesa import Model, agent
from mesa.time import RandomActivation
from mesa.space import MultiGrid
from mesa.datacollection import DataCollector

from rowAgent import RowAgent
from buildingAgent import BuildingAgent
from trafficLightAgent import TrafficLightAgent
from carAgent import CarAgent


classDict = {
    ">": RowAgent,
    "<": RowAgent,
    "v": RowAgent,
    "^": RowAgent,
    "#": BuildingAgent,
    "D": BuildingAgent,
    "S": TrafficLightAgent,
    "s": TrafficLightAgent

}

rowDirections = {
    "<": "-x",
    ">": "+x",
    "v": "-y",
    "^": "+y"
}

class Graph:
    # Retirved FROM : 
    def __init__(self, adjac_lis):
        self.adjac_lis = adjac_lis
 
    def get_n(self, v):
        return self.adjac_lis[v]
 
    # This is heuristic function which is having equal values for all nodes
    def h(self, n):
        H = {
            'A': 1,
            'B': 1,
            'C': 1,
            'D': 1
        }
 
        return 1

    def a_star_algorithm(self, start, stop):
        # In this open_lst is a lisy of nodes which have been visited, but who's 
        # neighbours haven't all been always inspected, It starts off with the start 
  #node
        # And closed_lst is a list of nodes which have been visited
        # and who's neighbors have been always inspected
        open_lst = set([start])
        closed_lst = set([])
 
        # poo has present distances from start to all other nodes
        # the default value is +infinity
        poo = {}
        poo[start] = 0
 
        # par contains an adjac mapping of all nodes
        par = {}
        par[start] = start
 
        while len(open_lst) > 0:
            n = None
 
            # it will find a node with the lowest value of f() -
            for v in open_lst:
                if n == None or poo[v] + self.h(v) < poo[n] + self.h(n):
                    n = v;
 
            if n == None:
                print('Path does not exist!')
                return None
 
            # if the current node is the stop
            # then we start again from start
            if n == stop:
                reconst_path = []
 
                while par[n] != n:
                    reconst_path.append(n)
                    n = par[n]
 
                reconst_path.append(start)
 
                reconst_path.reverse()
 
                print('Path found: {}'.format(reconst_path))
                return reconst_path
 
            # for all the neighbors of the current node do
            for (m, weight) in self.get_n(n):
              # if the current node is not presentin both open_lst and closed_lst
                # add it to open_lst and note n as it's par
                if m not in open_lst and m not in closed_lst:
                    open_lst.add(m)
                    par[m] = n
                    poo[m] = poo[n] + weight
 
                # otherwise, check if it's quicker to first visit n, then m
                # and if it is, update par data and poo data
                # and if the node was in the closed_lst, move it to open_lst
                else:
                    if poo[m] > poo[n] + weight:
                        poo[m] = poo[n] + weight
                        par[m] = n
 
                        if m in closed_lst:
                            closed_lst.remove(m)
                            open_lst.add(m)
 
            # remove n from the open_lst, and add it to closed_lst
            # because all of his neighbors were inspected
            open_lst.remove(n)
            closed_lst.add(n)
 
        print('Path does not exist!')
        return None

class CarsModel(Model):
    """
    Model class 
    input:file
    output: model
    """
    def __init__(self, file,width,height):
        """
        Initialize the model
        input: file
        output: model
        """
        #Grid
        self.grid = MultiGrid(width,height,False)
        #Schedule
        self.schedule = RandomActivation(self)
        #Running
        self.running = True
        #Posible destinations
        self.destinations = []
        #Read file
        self.setup(file)
        #Adj list
        self.adjList = {}
        #Create adj list
        self.createAdjList()
        #Graph
        self.graph = Graph(self.adjList)
        #Posible cars start points
        self.corners = [
            (0,0),
            (0,self.grid.height-1),
            (self.grid.width-1,self.grid.height-1),
            (self.grid.width-1,1)
        ]

        
    
    def setup(self,file):
        """
        Setup the agents in respective files
        input: file
        output: none
        """
        with open(file) as f:
            data = f.read()
        data = data.split("\n")
        for y,row in enumerate(data):
            for x,cell in enumerate(row):
                if cell in rowDirections:
                    #Crate agent
                    agent = classDict[cell](f"{x}{y}{cell}",self,rowDirections[cell]) 
                else:
                    if classDict[cell] == BuildingAgent:
                        #Crate agent
                        agent = classDict[cell](f"{x}-{y}{cell}",self,False)
                        if cell == "D":
                            agent.destination = True
                            self.destinations.append((x,self.grid.height-y-1))
                    else: 
                        if classDict[cell] == TrafficLightAgent:
                            onId = 1 if cell == "S" else 2
                            #Crate agent
                            agent = classDict[cell](f"{x}-{y}{cell}",self,onId)
                #Add agent to the grid
                self.grid.place_agent(agent,(x,self.grid.height-y-1))
                #Add agent to the schedule
                self.schedule.add(agent)

    def createAdjList(self):
        """
        Create adjacency list
        input: none
        output: none
        """
        for agents,x,y in self.grid.coord_iter():
            rowA = self.hasAgent(agents,RowAgent)
            destA = self.hasAgent(agents,BuildingAgent)
            if rowA:
                self.adjList[f"{x}-{y}"] = []
                for neighbor in self.grid.get_neighbors((x,y),moore=True,include_center=False, radius=1):
                    if isinstance(neighbor,RowAgent):
                        nX,nY = neighbor.pos
                        nD = neighbor.direction
                        if rowA.direction == "+x":
                            if (nX > x and nD == "+x" )or( nY >= y and nD == "+y") or( nY <= y and nD == "-y"):
                                self.adjList[f"{x}-{y}"].append((f"{nX}-{nY}",1))
                        elif rowA.direction == "-x":
                            if (nX < x and nD == "-x" )or( nY >= y and nD == "+y" )or( nY <= y and nD == "-y"):
                                self.adjList[f"{x}-{y}"].append((f"{nX}-{nY}",1))
                        elif rowA.direction == "+y":
                            if (nY > y and nD == "+y") or (nX >= x and nD == "+x") or (nX <= x and nD == "-x"):
                                self.adjList[f"{x}-{y}"].append((f"{nX}-{nY}",1))
                        elif rowA.direction == "-y":
                            if (nY < y and nD == "-y") or (nX >= x and nD == "+x") or (nX <= x and nD == "-x"):
                                self.adjList[f"{x}-{y}"].append((f"{nX}-{nY}",1))
                    elif isinstance(neighbor,TrafficLightAgent):
                        nX,nY = neighbor.pos
                        dir = rowA.direction
                        if (dir == "+x" or dir == "-x") and y == nY:
                            if nX > x :
                                self.adjList[f"{x}-{y}"].append((f"{nX+1}-{nY}",1)) 
                            elif nX < x:
                                self.adjList[f"{x}-{y}"].append((f"{nX-1}-{nY}",1))
                        elif (dir == "+y" or dir == "-y") and x == nX:
                            if nY > y:
                                self.adjList[f"{x}-{y}"].append((f"{nX}-{nY+1}",1))
                            elif nY < y:
                                self.adjList[f"{x}-{y}"].append((f"{nX}-{nY-1}",1))              
                    elif isinstance(neighbor,BuildingAgent):
                        if neighbor.destination:
                            nX,nY = neighbor.pos
                            self.adjList[f"{x}-{y}"].append((f"{nX}-{nY}",1))      
            elif destA:
                if destA.destination:
                    self.adjList[f"{x}-{y}"] = []
    def hasAgent(self,arr,agent):
        """
        Check if the agent is in the array
        input: array,agent
        output: boolean
        """
        for a in arr:
            if isinstance(a,agent):
                return a
        return False

    def placeCar(self):
        #Crate agent
        x,y = random.choice(self.corners)
        agent =CarAgent(f"{self.random.random() }-car",self,(x,y))
        #Random corner
        #Add agent to the grid
        self.grid.place_agent(agent,(0,self.grid.height-0-1))
        #Add agent to the schedule
        self.schedule.add(agent)

    def getCars(self):
        """
        Get all the cars
        input: none
        output: array
        """
        return [{"x":robot.pos[0],"y":0,"z":robot.pos[1],"id":robot.unique_id} for robot in self.schedule.agents if type(robot) is CarAgent]

    def getTrafficLights(self):
        """
        Get all the traffic lights
        input: none
        output: array
        """
        return [{"x":robot.pos[0],"y":0,"z":robot.pos[1],"status":robot.status,"id":f"{robot.pos[0]}-{robot.pos[1]}"} for robot in self.schedule.agents if type(robot) is TrafficLightAgent]

    def step(self):
        """
        Step function
        input: none
        output: none
        """
        #Place car in each frame
        possibility = self.random.random()
        if possibility < 1:
            self.placeCar()

        #Advance the schedule
        self.schedule.step()