using System;
using System.Collections.Generic;
using System.Text;

public class Camel: ICloneable
{
    public char Id { get; private set; }
    public int Pos { get; set; }
    public Camel CamelOnTop{ get; set; }
    public bool IsMoving { get; set; }

    public Camel(char aId)
    {
        Id = aId;
    }

    public Camel(char aId, int aPos, Camel aCamelOnTop) : this(aId)
    {
        this.Pos = aPos;
        this.CamelOnTop = aCamelOnTop;
    }

    public object Clone()
    {
        return new Camel(Id, Pos, CamelOnTop);
    }
}

public class Trap
{
    public bool IsPlusTrap { get; private set; }
    public int Pos { get; private set; }

    public Trap(char kind, int pos)
    {
        IsPlusTrap = kind == GameRules.TRAP_PLUS;
        Pos = pos;
    }
}

public class CamelsMovement
{
    private List<Camel> m_StartingCamels = new List<Camel>();
    private List<Camel> m_Camels = new List<Camel>();
    private List<Trap> m_Traps = new List<Trap>();  

    public string StartingCamelsInBoard { get { return GetBoard(m_StartingCamels); } }

    public CamelsMovement(string aBoard)
    {
        InitiateBoard(aBoard);
    }

    public List<string> GetCamelsResults()
    {
        List<string> retval = new List<string>();

        for(int i = 0; i < m_StartingCamels.Count; i++)
        {
            for(int j = 1; j <= GameRules.DICE_NB_FACES; j++)
            {
                m_Camels = CloneCamels(m_StartingCamels);
                MoveCamel(m_Camels[i], j, true);
                //TODO info hardcoder
                retval.Add(string.Format("{0}{1}{2}{3}",m_Camels[i].Id, j, GameRules.PATTERN_RESULT_NAME_SEPARATOR, GetBoard(m_Camels)));
            }
        }

        return retval;
    }

    private void InitiateBoard(string aBoard)
    {
        m_StartingCamels.Clear();
        m_Traps.Clear();

        string[] subPattern = aBoard.Split(GameRules.CASE_SEPARATOR);

        for (int pos = 0; pos < subPattern.Length; pos++)
        {
            Camel lastCamel = null;
            string line = subPattern[pos];

            for (int j = line.Length - 1; j >= 0; j--)
            {
                if (line[j] == GameRules.TRAP_PLUS || line[j] == GameRules.TRAP_MINUS)
                {
                    m_Traps.Add(new Trap(line[j], pos));
                }
                else
                {
                    Camel newCamel = new Camel(line[j], pos, lastCamel);
                    m_StartingCamels.Add(newCamel);
                    lastCamel = newCamel;
                }
            }
        }
    }

    private void RemoveCamelOnTop(Camel aCamel)
    {
        for (int i = 0; i < m_Camels.Count; i++)
        {
            if (m_Camels[i].CamelOnTop != null && m_Camels[i].CamelOnTop.Id == aCamel.Id)
            {
                m_Camels[i].CamelOnTop = null;
            }
        }
    }

    private void CheckCamelLandOnAnotherCamel(Camel aCamel, bool aFromMinusTrap)
    {
        if (!aFromMinusTrap)
        {
            for (int i = 0; i < m_Camels.Count; i++)
            {
                if (m_Camels[i] != aCamel &&
                    m_Camels[i].Pos == aCamel.Pos &&
                    m_Camels[i].CamelOnTop == null)                
                        m_Camels[i].CamelOnTop = aCamel;                  
            }
        }
        else
        {
            //TODO faire des tests
            string camelsOnTop = aCamel.Id.ToString();
            string camelsOnPos = string.Empty;
            List<Camel> orderedStartingCamel = SortCamelInOrderPos(m_Camels);
            Camel tempCamel = aCamel.CamelOnTop;

            while(tempCamel != null)
            {
                camelsOnTop += tempCamel.Id;
                tempCamel = tempCamel.CamelOnTop;
            }             

            for (int i = 0; i < orderedStartingCamel.Count; i++)
            {
                if (!camelsOnTop.Contains(orderedStartingCamel[i].Id.ToString()) 
                    && orderedStartingCamel[i].Pos == aCamel.Pos
                    && !GetCurrentCamel(orderedStartingCamel[i].Id).IsMoving)
                    camelsOnPos += orderedStartingCamel[i].Id;
            }

            if (!string.IsNullOrEmpty(camelsOnPos))
            {
                aCamel.CamelOnTop = GetCurrentCamel(camelsOnPos[camelsOnPos.Length - 1]);

                if (aCamel.CamelOnTop == null || aCamel.CamelOnTop == aCamel)
                    GameRules.Log("Peut etre un bug....");
            }               
        }
    }

    private string GetBoard(List<Camel> aCamel)
    {
        string retval = string.Empty;
        List<Camel> camels = SortCamelInOrderPos(aCamel);

        retval = retval.PadLeft(camels[camels.Count - 1].Pos, GameRules.CASE_SEPARATOR);
        int lastPos = camels[camels.Count - 1].Pos;
        
        for (int i = camels.Count - 1; i >= 0; i--)
        {
            int diff = camels[i].Pos - lastPos;

            for (int j = 0; j < diff; j++)
                retval += GameRules.CASE_SEPARATOR;

            retval += camels[i].Id;
            lastPos = camels[i].Pos;
        }

        for (int i = 0; i < m_Traps.Count; i++)
        {
            int pos = 0;
            int posIndex = 0;
            char trap = m_Traps[i].IsPlusTrap ? GameRules.TRAP_PLUS : GameRules.TRAP_MINUS;

            for(int j = 0; pos != m_Traps[i].Pos; j++)
            {
                if (j >= retval.Length)
                    retval += GameRules.CASE_SEPARATOR;

                if (retval[j] == GameRules.CASE_SEPARATOR)
                    pos++;

                posIndex = j + 1;
            }

            retval = retval.Insert(posIndex, trap.ToString());
        }
        
        return retval;
    }

    private Camel GetCurrentCamel(char aId)
    {
        for (int i = 0; i < m_Camels.Count; i++)
        {
            if (m_Camels[i].Id == aId)
                return m_Camels[i];
        }

        return null;
    }

    private List<Camel> SortCamelInOrderPos(List<Camel> aCamel)
    {
        List<Camel> newList = new List<Camel>();
        List<Camel> remainingCamels = (List<Camel>)Extensions.Clone(aCamel);

        for (int j = 0; j < aCamel.Count; j++)
        {
            char tempCamelName = 'x';
            Camel higherCamel = new Camel(tempCamelName, -1, null);

            for (int i = 0; i < remainingCamels.Count; i++)
            {
                Camel currentCamel = remainingCamels[i];

                if (newList.Count > 0 && currentCamel.CamelOnTop != null && currentCamel.CamelOnTop.Id == newList[newList.Count - 1].Id)
                {
                    //prend le camelOnTop du dernier camel entrer
                    higherCamel = currentCamel;
                    break;
                }
                else
                {
                    //Prend le plus grosse pos + sans camel on top
                    if (currentCamel.CamelOnTop == null && currentCamel.Pos > higherCamel.Pos)
                    {
                        higherCamel = currentCamel;
                    }
                }
            }

            for (int k = 0; k < remainingCamels.Count; k++)
            {
                if (remainingCamels[k].Id == higherCamel.Id)
                    remainingCamels.Remove(remainingCamels[k]);
            }

            if (higherCamel.Id == tempCamelName)
            {
                GameRules.Log("Didnt find higherCamel");
            }

            newList.Add(higherCamel);
        }

        if (remainingCamels.Count != 0)
        {
            GameRules.Log("We miss a Camel");
        }

        return newList;
    }

    private void CamelInfo(List<Camel> aCamels, string aPreLog)
    {
        string log = aPreLog + '\n';

        for(int i = 0; i < aCamels.Count; i++)
        {
            log += string.Format("{0}: {1} -> {2} {3}", aCamels[i].Id, aCamels[i].Pos, aCamels[i].CamelOnTop == null ? "null": aCamels[i].CamelOnTop.Id.ToString(), '\n');
        }

        GameRules.Log(log);
    }

    private List<Camel> CloneCamels(List<Camel> aCamels)
    {
        List<Camel> retval = (List<Camel>)Extensions.Clone(m_StartingCamels);

        //Assign CamelOnTop with camel in the newList
        for(int i = 0; i < retval.Count; i++)
        {
            if(retval[i].CamelOnTop != null)
            {             
                for(int j = 0; j < retval.Count; j++)
                {
                    if (retval[i].CamelOnTop.Id == retval[j].Id)
                    {
                        retval[i].CamelOnTop = retval[j];
                        break;
                    }
                }
            }
        }

        return retval;
    }

    private void IsLandingOnTrap(Camel aCamel)
    {
        for(int i = 0; i < m_Traps.Count; i++)
        {
            if(m_Traps[i].Pos == aCamel.Pos)
            {
                //TODO Trap: check minus test && IS_SHUTTLE_WHEN_HITTING_MINUS_TRAP
                int modifMovement = m_Traps[i].IsPlusTrap ? GameRules.TRAP_PLUS_MODIFIER : GameRules.TRAP_MINUS_MODIFIER;
                //MoveCamel(aCamel, modifMovement, true);
	            aCamel.Pos += modifMovement;
	            CheckCamelLandOnAnotherCamel(aCamel, modifMovement < 0);
			}
        }
    }

	private void MoveCamel(Camel aCamel, int aDice, bool aIsFirstCamel)
	{
		aCamel.IsMoving = true;
		aCamel.Pos += aDice;

		if (aIsFirstCamel)
		{
			RemoveCamelOnTop(aCamel);
		}

		IsLandingOnTrap(aCamel);

		CheckCamelLandOnAnotherCamel(aCamel, aDice < 0);		

		if (aCamel.CamelOnTop != null && aDice > 0)
		{
			MoveCamel(aCamel.CamelOnTop, aDice, false);
		}

		aCamel.IsMoving = false;
	}
}
