from mesa import Agent

class BuildingAgent(Agent):
    """
    Agent class for building
    input: unique id, model
    output: building agent
    """

    def __init__(self,id,model,destination) -> None:
        """
        Inicialize the building agent
        input: id,model
        """
        super().__init__(id,model)
        self.destination = destination
    
    def isDestination(self):
        """
        Check if the building is a destination
        input: none
        output: boolean
        """
        return self.destination

    def step(self):
        """
        Step function
        """
        pass

