import random
from mesa import Agent
from rowAgent import RowAgent
from trafficLightAgent import TrafficLightAgent

class CarAgent(Agent):
    def __init__(self,id,model) -> None:
        super().__init__(id,model)
        self.destination = ""
        self.chooseDestination()

        self.route = []

        self.calculateRoute()

    def calculateRoute(self):
        self.route = self.model.graph.a_star_algorithm('0-0', self.destination)
        
    
    def continueRoute(self):
        if len(self.route) > 0:
            x,y = [int(x) for x in self.route[0].split("-")]
            self.model.grid.move_agent(self,(x,y))
            self.route.pop(0)

    def listContents(self,x,y):
        return self.model.grid.get_cell_list_contents([(x,y)])
    
    def getRowDirection(self):
        x,y = self.pos
        agents = self.listContents(x,y)
        for agent in agents:
            if isinstance(agent,RowAgent):
                return agent.direction
        pass
    
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
    
    def isInTrafficLight(self):
        x,y = self.pos
        dir = self.getRowDirection()
        if dir == "+x":
            traffic = self.hasAgent(self.listContents(x+1,y),TrafficLightAgent)
            if  traffic and traffic.status == False:
                return True
        elif dir == "-x":
            traffic = self.hasAgent(self.listContents(x-1,y),TrafficLightAgent)
            if  traffic and traffic.status == False:
                return True
        elif dir == "+y":
            traffic = self.hasAgent(self.listContents(x,y+1),TrafficLightAgent)
            if  traffic and traffic.status == False:
                return True
        elif dir == "-y":
            traffic = self.hasAgent(self.listContents(x,y-1),TrafficLightAgent)
            if  traffic and traffic.status== False:
                return True
        return False
    def move(self):
        if  self.isInTrafficLight():
            return
        if self.trafficInRoute():
            self.recalculateRoute()
            return
        self.continueRoute()
        return
    def carInDestination(self):
        x,y = self.pos
        xDes,yDes = [int(x) for x in self.destination.split("-")]
        return  x == xDes and y == yDes
        
    def chooseDestination(self):
        x,y = random.choice(self.model.destinations)
        self.destination = f"{x}-{y}"

    def step(self):
        if self.carInDestination():
            self.model.grid.remove_agent(self)
            self.model.schedule.remove(self)
        else:
            self.move()
    
    def trafficInRoute(self):
        xNext,yNext = [int(x) for x in self.route[0].split("-")]
        contents = self.listContents(xNext,yNext)
        for content in contents:
            if isinstance(content,CarAgent):
                return True
    
    def availableRoutes(self):
        x,y = self.pos
        possiblePlaces = self.model.adjList[f"{x}-{y}"]
        availablePlaces = []
        for place in possiblePlaces:
            pos = [int(x) for x in place[0].split("-")]
            if len(self.listContents(pos[0],pos[1])) == 1:
                availablePlaces.append(pos)
        return availablePlaces
    
    def recalculateRoute(self):
        print("recalculating")
        ap = self.availableRoutes()
        if len(ap) > 0:
            print("HAS OPTIONS")
            x,y = random.choice(ap)
            posibleRoute = self.model.graph.a_star_algorithm(f"{x}-{y}", self.destination)
            if posibleRoute:
                self.route = posibleRoute
            self.continueRoute()