using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;

namespace WCS_TASK_SC
{
    public class cDbBaseOra
    {
        #region@@@.변수 객체선언[OleDb]
        public  OleDbConnection mCnMain;                                // @.외부에서 할당해주는 Connection 개체 참조(Connection이 하나 일 경우)
        public OleDbTransaction mTrnMain;                              // @.자체 생성되는 DB 객체들
        public OleDbCommand mComMain = new OleDbCommand();             // @.자체 생성되는 DB 객체들

        public bool mIsBeginTran = false;                             // @.Begin Tran 여부
        public OleDbDataAdapter mDaMain = new OleDbDataAdapter();      // @.Data Adapter
        public bool mBindingType;                                      // @.바인딩 객체에 사용할지 여부[바인딩 객체일 경우 Reset을 하면 안됨.(계속 연결된 상태, DataSource에 따라 작동)]                                 // @.SQL Command
        #endregion

        #region@@@.상수 선언
        public const int DB_ERR  = -1;      // @.DB 에러
        public const int DB_LOCK = -2;      // @.DB 에러중 DB Lock
        public const int DB_DUP = -3;       // @.DB 에러중 중복 데이타
        #endregion

        #region @@@.속성[DtMain]
        public DataTable mDtMain = new DataTable("Default");            // @.자체 생성되는 DB 객체들
        public DataTable DtMain
        {
            get
            {
                return DtMain;
            }
        }
        #endregion

        #region @@@.속성[DbConnted]
        public bool mDbConnted = false;    // @.DB Connection 성공여부
        public bool DbConnted
        {
            get
            {
                return mDbConnted;
            }
            set
            {
                mDbConnted = value;
            }
        }
        #endregion

        #region @@@.속성[ErrMsg]
        private string mErrMsg = "";      // @.DB Error Message
        public string ErrMsg 
        {
          get {
            return mErrMsg;
          }
          set {
            mErrMsg = value;
          }
        }
        #endregion

        #region @@@.속성[ErrKind]
        private int mErrKind = 0;         // @.DB Error 종류
        public int ErrKind 
        {
          get {
            return mErrKind;
          }
        }
        #endregion

        #region @@@.생성자
        // @.자체 Connection 객체 사용
        // @.외부에서 New 생성 후 Connection 객체를 Open하고 Init을 호출한다.
        // @.종료시 mComMain.Close를 반드시 호출
        public cDbBaseOra() 
        {
            this.mBindingType = false; 
        }

        public cDbBaseOra(bool bBind) 
        {
            this.mBindingType = bBind; 
        }

        public cDbBaseOra(ref OleDbConnection pConObj) 
        {
            this.mBindingType = false;
            this.mCnMain = pConObj;
            this.Init();
        }

        public cDbBaseOra(ref OleDbConnection pConObj, bool bBind) 
        {
            this.mBindingType = bBind;
            this.mCnMain = pConObj;
            this.Init();
        }
        #endregion

        public void DbBaseOra()
        {
            this.mBindingType = false;
        }

        public void DbBaseOra(bool bBind)
        {
            this.mBindingType = bBind;
        }

        public void DbBaseOra(ref OleDbConnection pConObj)
        {
            this.mBindingType = false;
            this.mCnMain = pConObj;
            this.Init();
        }

        public void DbBaseOra(ref OleDbConnection pConObj, bool bBind)
        {
            this.mBindingType = bBind;
            this.mCnMain = pConObj;
            this.Init();
        }

        #region@@@.DB init
        public void Init()
        {
            mComMain.Connection = mCnMain;
            mComMain.CommandType = CommandType.Text;
            mDaMain.SelectCommand = mComMain;
            mDbConnted = true;
            // 원래 용도는 None query일 경우 따로 DataCommand을 쓴다.
            //        comNonSel.Connection = gconDb
            //       comNonSel.CommandType = CommandType.Text
        }
        #endregion

        #region@@@.DB Error Message[프로젝트 별로 메세지를 표시하는 방법을 패생 클래스에서 오버라이드 해서 사용한다.]
        //* 프로젝트 별로 메세지를 표시하는 방법을 패생 클래스에서 오버라이드 해서 사용한다.
        public static void ShowErrMsg(bool  bMsgBox)
        {
            if (bMsgBox == true)
            {
                ////MessageBox.Show(ErrMsg, "DB Err||", MessageBoxButtons.OK, MessageBoxIcon.Err||)
            } 
        }
        #endregion

        #region@@@.쿼리 실행 For Select..  
        // @@.Parameter
        // @.pIsMsgBox (메세지 박스 표시 여부)
        // @.pIsRtnErr (에러 발생시, 에러를 Return할 지 여부)
        // @@.Return
        // @.성공 - 쿼리한 레코드 수 (양의 정수)
        // @.실패 - DB_ERR(-1):  일반 DB Err
        // @.실패 - DB_LOCK(-2): DB Lock
        // @.실패 - DB_DUP(-3):  데이타 중복
        public int ExcuteQry(string  strQry) 
        {
            return ExcuteQry( strQry, true , false);
        }
        public int ExcuteQry(string  strQry, bool bMsgBox) 
        {
            return ExcuteQry( strQry, bMsgBox, false);
        }
        public int ExcuteQry(string  strQry  , bool bMsgBox, bool breturnErr) 
        {
            mErrKind = 0;
            try
            {
                // 바인딩 객체일 경우 연결유지, DATA만 클리어
                if (mBindingType == true)
                {
                    mDtMain.Clear();
                } 
                else 
                {
                    mDtMain.Reset();
                }
                mComMain.CommandText = strQry;
                return mDaMain.Fill(mDtMain);
            }
            catch (OleDbException DbErr)
            {
                if (breturnErr) 
                {
                    throw DbErr;
                }
                else 
                {
                    mErrMsg = DbErr.Message;
                    if (mErrMsg.IndexOf("||A-03114") != -1 || mErrMsg.IndexOf("||A-12560") != -1)
                    {
                        mDbConnted = false ;
                    }  
                    
                    if (mErrMsg.IndexOf("||A-00054") != -1 )
                    {
                        // No wait 를 사용할 경우
                        mErrKind = DB_LOCK;
                    }
                    else
                    {
                        mErrKind = 0;
                        ////ShowErrMsg(bMsgBox);
                    }
                }
            return DB_ERR;
            }
        }
        #endregion

        #region@@@.쿼리 실행 For Select..  [2개 이상 쿼리를 할 경우 datatable을 별도로 바인딩 한다.]
        // @@.Parameter
        // @.bRerutnErr[에러 발생시, 에러를 Return할 지 여부]
        // @@.Return
        // @.성공 - 쿼리한 레코드 수 (양의 정수)
        // @.실패 - DB_ERR(-1):  일반 DB Err
        // @.실패 - DB_LOCK(-2): DB Lock
        // @.실패 - DB_DUP(-3):  데이타 중복
        public int ExcuteQry(ref DataTable dtOther, string  strQry)  
        {
            return ExcuteQry(ref dtOther, strQry, true, false);
        }
        public int ExcuteQry(ref DataTable dtOther, string  strQry, bool bMsgBox)  
        {
            return ExcuteQry(ref dtOther, strQry, bMsgBox, false);
        }
        public int ExcuteQry(ref DataTable dtOther, string  strQry, bool bMsgBox, bool breturnErr)  
        {
            mErrKind = 0;
            try
            {
            
                // 바인딩 객체일 경우 연결유지, DATA만 클리어
                if (mBindingType == false ) 
                {
                    dtOther.Clear();
                }
                else
                {
                    dtOther.Reset();
                }

                mComMain.CommandText = strQry;
                return mDaMain.Fill(dtOther);
            }
            catch( OleDbException DbErr)
            {
                if (breturnErr == true ) 
                {
                    throw DbErr;
                }
                else
                {
                    mErrMsg = DbErr.Message;

                    if (mErrMsg.IndexOf("||A-03114") != -1 || mErrMsg.IndexOf("||A-12560") != -1) 
                    { 
                        mDbConnted = false;
                    }

                    if (mErrMsg.IndexOf("||A-00054") != -1) 
                    {
                        // No wait 를 사용할 경우
                        mErrKind = DB_LOCK;
                    }
                    else
                    {
                        mErrKind = 0;
                    }
                    ////ShowErrMsg(bMsgBox)
                }
            }
            return DB_ERR;
        }
        #endregion
        
        #region@@@.None Query For insert, update, ...
        // @@.Parameter
        // @.bRerutnErr[에러 발생시, 에러를 Return할 지 여부]
        // @@.Return
        // @.성공 - 반영된 레코드 수 (양의 정수)
        // @.실패 - DB_ERR(-1):  일반 DB Err
        // @.실패 - DB_LOCK(-2): DB Lock
        // @.실패 - DB_DUP(-3):  데이타 중복
        public int  ExcuteNonQry(string  strQry) 
        {
            return ExcuteNonQry(strQry, true, false);
        }
        public int ExcuteNonQry(string strQry, bool bMsgBox)
        {
            return ExcuteNonQry(strQry, bMsgBox, false);
        }
        public int  ExcuteNonQry(string  strQry, bool bMsgBox, bool breturnErr) 
        {
            mErrKind = 0;
            try
            {
                mComMain.CommandText = strQry;
                return mComMain.ExecuteNonQuery();
            }
            catch(OleDbException DbErr)
            {
                if (breturnErr == true) 
                {
                    throw DbErr;
                }
                else
                {
                    mErrMsg = DbErr.Message;
                    if (mErrMsg.IndexOf("||A-03114") != -1 || mErrMsg.IndexOf("||A-12560") != -1) 
                    { 
                        mDbConnted = false;
                    }

                    if (mErrMsg.IndexOf("||A-00001") != -1)
                    {
                        mErrKind = DB_DUP;
                    }
                    else
                    {
                        mErrKind = 0;
                    }
                    ////ShowErrMsg(bMsgBox)
                }
            }
            return DB_ERR;
        }
        #endregion
        
        #region@@@.Transction 객체 할당
        public void BeginTrans()
        {
            try
            {
                    mTrnMain = mCnMain.BeginTransaction();
                mIsBeginTran = true;
            }
            
            catch (Exception AppErr)
            {
                throw AppErr;
            }
        }
        #endregion

        #region@@@.Transction Commit
        public void Commit()
        {
            try
            {
                if (mIsBeginTran == true) 
                {
                    mTrnMain.Commit();
                    mIsBeginTran = false;
                }
                
            }
            catch (Exception AppErr)
            {
                throw AppErr;
            }
        }
        #endregion

        #region@@@.Transction Rollback
        public void Rollback()
        {
            try
            {
                if (mIsBeginTran == true)
                {
                    mTrnMain.Rollback();
                    mIsBeginTran = false;
                }
            }
            catch(Exception AppErr)
            {
                throw AppErr;
            }
        }
        #endregion
        
        #region@@@.프로시저 사용할 경우 셋팅
        public void SetProcedure(string  pSp_Name)
        {
            mComMain.CommandType = CommandType.StoredProcedure;
            mComMain.CommandText = pSp_Name;
            mComMain.Parameters.Clear();
        }
        #endregion
    }
}
