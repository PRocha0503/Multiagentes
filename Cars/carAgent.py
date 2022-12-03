import random
from mesa import Agent
from rowAgent import RowAgent
from trafficLightAgent import TrafficLightAgent

class CarAgent(Agent):
    def __init__(self,id,model,start) -> None:
        super().__init__(id,model)
        self.destination = ""
        self.chooseDestination()

        self.route = []

        self.calculateRoute(start)

    def calculateRoute(self,start):
        self.route = self.model.graph.a_star_algorithm(f'{start[0]}-{start[1]}', self.destination)
        if not self.route:
            print("ERROR")
            self.model.grid.remove_agent(self)
            self.model.schedule.remove(self)

           
    
    def continueRoute(self):
        if len(self.route) > 0:
            x,y = [int(x) for x in self.route[0].split("-")]
            self.model.grid.move_agent(self,(x,y))
            self.route.pop(0)
        else:
            self.model.grid.remove_agent(self)
            self.model.schedule.remove(self)

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
        xNext,yNext = [int(x) for x in self.route[0].split("-")]
        prox = {xNext, yNext}
        if prox ==  {x+2,y} and self.hasAgent(self.listContents(x+1, y),TrafficLightAgent) and self.hasAgent(self.listContents(x+1, y),TrafficLightAgent).status == False:
            return True
        elif prox ==  {x-2,y} and self.hasAgent(self.listContents(x-1, y),TrafficLightAgent) and self.hasAgent(self.listContents(x-1, y),TrafficLightAgent).status == False:
            return True
        elif prox ==  {x,y+2} and self.hasAgent(self.listContents(x, y+1),TrafficLightAgent) and self.hasAgent(self.listContents(x, y+1),TrafficLightAgent).status == False:
            return True
        elif prox ==  {x,y-2} and self.hasAgent(self.listContents(x, y-1),TrafficLightAgent) and self.hasAgent(self.listContents(x, y-1),TrafficLightAgent).status == False:
            return True
        return False
    
    def move(self):
        if not self.route: return
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
        occupiedPlaces = []
        for place in possiblePlaces:
            pos = [int(x) for x in place[0].split("-")]
            if len(self.listContents(pos[0],pos[1])) == 1:
                availablePlaces.append(pos)
            else: 
                occupiedPlaces.append(pos)
        return availablePlaces, occupiedPlaces
    
    def recalculateRoute(self):
        ap,op = self.availableRoutes()
        if len(ap) > 0:
            print("HAS OPTIONS")
            x,y = random.choice(ap)
            posibleRoute = self.model.graph.a_star_algorithm(f"{x}-{y}", self.destination)
            if posibleRoute :
                for o in op:
                    if o in posibleRoute:
                        self.continueRoute()
                self.route = posibleRoute
            self.continueRoute()